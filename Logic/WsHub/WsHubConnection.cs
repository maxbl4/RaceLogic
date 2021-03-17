using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.WsHub.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubConnection : IAsyncDisposable, IPingRequester
    {
        private readonly CompositeDisposable disposable = new();

        private readonly ConcurrentDictionary<Id<Message>, DateTime> lastSeenMessageIds = new();
        private readonly ILogger logger = Log.ForContext<WsHubConnection>();
        private readonly WsHubClientOptions options;

        private readonly ConcurrentDictionary<Id<Message>, TaskCompletionSource<Message>>
            outstandingClientRequests = new();

        private readonly ISystemClock systemClock;
        private readonly ConcurrentDictionary<string, string> topicSubscriptions = new();
        private readonly BehaviorSubject<WsConnectionStatus> webSocketConnected = new(new WsConnectionStatus());
        private volatile bool disposed;
        private HubConnection wsConnection;

        public WsHubConnection(WsHubClientOptions options, ISystemClock systemClock = null)
        {
            this.options = options;
            this.systemClock = systemClock ?? new DefaultSystemClock();
            _ = CleanupSeenMessageIds();
            RegisterRequestHandler<PingRequest>(ping => Task.FromResult<Message>(new PingResponse
            {
                SenderTimestamp = ping.Timestamp
            }));
        }

        public IObservable<WsConnectionStatus> WebSocketConnected => webSocketConnected;
        public Func<Message, Task> MessageHandler { get; set; }
        public ConcurrentDictionary<Type, Func<IRequestMessage, Task<Message>>> RequestHandlers { get; } = new();

        public ValueTask DisposeAsync()
        {
            disposed = true;
            disposable.DisposeSafe();
            logger.Swallow(() => wsConnection.DisposeAsync());
            return ValueTask.CompletedTask;
        }

        public async Task Connect()
        {
            wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{this.options.Address}_ws/hub",
                    options => { options.AccessTokenProvider = () => Task.FromResult(this.options.AccessToken); })
                .Build();
            wsConnection.Closed += exception =>
            {
                logger.Information("Connection closed");
                _ = TryConnect();
                return Task.CompletedTask;
            };

            disposable.Add(wsConnection.On(nameof(IWsHubClient.ReceiveMessage),
                async (JObject msg) => await DispatchMessage(msg)));

            await TryConnect();
        }

        private async Task HandleRequest(IRequestMessage request, Func<IRequestMessage, Task<Message>> handler)
        {
            try
            {
                logger.Debug($"HandleRequest begin {request}");
                Message response;
                try
                {
                    response = await handler(request) ?? new UnhandledRequest();
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, $"Error handling request {request}");
                    response = new UnhandledRequest {Exception = ex};
                }

                response.MessageId = request.MessageId;
                await SendToDirect(request.SenderId, response);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, $"Error responding to request {request}");
                throw;
            }
        }

        private async Task DispatchMessage(JObject obj)
        {
            try
            {
                var msg = Message.MaterializeConcreteMessage(obj);
                if (lastSeenMessageIds.TryAdd(msg.MessageId, systemClock.UtcNow.DateTime))
                {
                    if (outstandingClientRequests.TryGetValue(msg.MessageId, out var tcs))
                    {
                        tcs.TrySetResult(msg);
                    }
                    else if (msg is IRequestMessage request)
                    {
                        RequestHandlers.TryGetValue(msg.GetType(), out var handler);
                        await HandleRequest(request, handler);
                    }
                    else if (MessageHandler != null)
                    {
                        await MessageHandler(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, $"Error dispatching message {obj}");
            }
        }

        public async Task SendToDirect(string targetId, Message msg)
        {
            msg.Target = new MessageTarget
            {
                Type = TargetType.Direct,
                TargetId = targetId
            };
            await SendCore(msg);
        }

        public async Task SendToTopic(string topicId, Message msg)
        {
            msg.Target = new MessageTarget
            {
                Type = TargetType.Topic,
                TargetId = topicId
            };
            await SendCore(msg);
        }

        public async Task SendCore(Message msg)
        {
            msg.MessageType = msg.GetType().FullName;
            await wsConnection.InvokeAsync(nameof(IWsHubServer.SendTo), msg);
        }

        public async Task<TResponse> InvokeRequest<TRequest, TResponse>(string targetId, TRequest msg)
            where TRequest : Message, IRequestMessage
            where TResponse : Message
        {
            msg.Target = new MessageTarget {Type = TargetType.Direct, TargetId = targetId};
            msg.MessageType = msg.GetType().FullName;
            var tcs = new TaskCompletionSource<Message>();
            try
            {
                if (!outstandingClientRequests.TryAdd(msg.MessageId, tcs))
                    throw new DuplicateRequestException(msg.MessageId);
                await SendToDirect(targetId, msg);
                var operationResult = await Task.WhenAny(tcs.Task, Task.Delay(msg.Timeout ?? TimeSpan.FromSeconds(30)));
                if (operationResult is Task<Message> successResult)
                {
                    var response = successResult.Result;
                    if (response is UnhandledRequest un)
                        throw new UnhandledRequestException(un.Exception);
                    return (TResponse) successResult.Result;
                }

                throw new TimeoutException($"Request {msg.MessageType} to {targetId} timed out after {msg.Timeout}");
            }
            finally
            {
                outstandingClientRequests.TryRemove(msg.MessageId, out _);
            }
        }

        public void RegisterRequestHandler<T>(Func<T, Task<Message>> handler)
            where T : Message, IRequestMessage
        {
            RequestHandlers[typeof(T)] = msg => handler((T) msg);
        }

        public IEnumerable<Id<Message>> GetOutstandingRequestIds()
        {
            return outstandingClientRequests.Keys.ToList();
        }

        public async Task RegisterService(ServiceFeatures serviceFeatures)
        {
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Register), new RegisterServiceMessage
            {
                Features = serviceFeatures
            });
            logger.Information($"Registered as {options.Features}");
        }

        public async Task<List<ServiceRegistration>> ListServiceRegistrations()
        {
            var response = await wsConnection
                .InvokeAsync<ListServiceRegistrationsResponse>(
                    nameof(IWsHubServer.ListServiceRegistrations),
                    new ListServiceRegistrationsRequest());
            return response.Registrations;
        }

        public async Task Subscribe(params string[] topicIds)
        {
            foreach (var topicId in topicIds) topicSubscriptions.TryAdd(topicId, topicId);
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Subscribe),
                new TopicSubscribeMessage
                {
                    TopicIds = topicIds
                });
        }

        public async Task Unsubscribe(params string[] topicIds)
        {
            foreach (var topicId in topicIds) topicSubscriptions.TryRemove(topicId, out _);
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Unsubscribe),
                new TopicSubscribeMessage
                {
                    TopicIds = topicIds
                });
        }

        private async Task HandleDisconnect(Exception ex)
        {
            webSocketConnected.OnNext(new WsConnectionStatus {Exception = ex});
            await Task.Delay(options.ReconnectTimeout);
            _ = TryConnect();
        }

        private async Task TryConnect()
        {
            try
            {
                if (wsConnection.State != HubConnectionState.Connected && wsConnection.State !=
                                                                       HubConnectionState.Connecting
                                                                       && !disposed)
                {
                    await wsConnection.StartAsync();
                    logger.Information("TryConnect success");
                    await RegisterService(options.Features);
                    await Subscribe(topicSubscriptions.Keys.ToArray());
                }

                webSocketConnected.OnNext(new WsConnectionStatus {IsConnected = true});
            }
            catch (Exception ex)
            {
                logger.Warning("TryConnect failed", ex);
                HandleDisconnect(ex).Wait(0);
            }
        }

        private async Task CleanupSeenMessageIds()
        {
            while (!disposed)
            {
                var oldMessageIds = lastSeenMessageIds
                    .Where(x => systemClock.UtcNow.DateTime - x.Value
                                > options.LastSeenMessageIdsRetentionPeriod).ToList();
                foreach (var oldMessageId in oldMessageIds) lastSeenMessageIds.TryRemove(oldMessageId.Key, out _);
                await Task.Delay(options.LastSeenMessageIdsCleanupPeriod);
            }
        }
    }
}