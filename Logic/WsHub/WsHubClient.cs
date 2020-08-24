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
using maxbl4.Infrastructure.Extensions.TaskExt;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.WsHub.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubClientOptions
    {
        public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan LastSeenMessageIdsRetentionPeriod { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan LastSeenMessageIdsCleanupPeriod { get; set; } = TimeSpan.FromSeconds(5);
        
        public static WsHubClientOptions Default => new WsHubClientOptions();
    }
    
    public class WsHubClient: IDisposable
    {
        public ServiceRegistration ServiceRegistration { get; }
        private volatile bool disposed = false;
        private readonly ILogger logger = Log.ForContext<WsHubClient>();
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly string address;
        private readonly WsHubClientOptions options;
        private readonly ISystemClock systemClock;
        private HubConnection wsConnection;
        private readonly BehaviorSubject<WsConnectionStatus> webSocketConnected = new BehaviorSubject<WsConnectionStatus>(new WsConnectionStatus());
        public IObservable<WsConnectionStatus> WebSocketConnected => webSocketConnected;
        
        private readonly ConcurrentDictionary<Id<Message>, DateTime> lastSeenMessageIds = new ConcurrentDictionary<Id<Message>, DateTime>();
        private readonly ConcurrentDictionary<string, string> topicSubscriptions = new ConcurrentDictionary<string, string>();
        private readonly Subject<Message> messages = new Subject<Message>();
        public IObservable<Message> Messages => messages;
        public Func<Message, Task<Message>> RequestHandler { get; set; }

        public WsHubClient(string address, ServiceRegistration serviceRegistration, WsHubClientOptions options = null, ISystemClock systemClock = null)
        {
            ServiceRegistration = serviceRegistration;
            if (!address.EndsWith("/"))
                address += "/";
            this.address = address;
            this.options = options ?? WsHubClientOptions.Default;
            this.systemClock = systemClock ?? new DefaultSystemClock();
            _ = CleanupSeenMessageIds();
        }

        public async Task Connect()
        {
            wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{address}ws/hub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(ServiceRegistration.ServiceId);
                })
                .Build();
            wsConnection.Closed += exception =>
            {
                logger.Information("Connection closed");
                _ = TryConnect();
                return Task.CompletedTask;
            };
            
            disposable.Add(Disposable.Create(() => logger.Swallow(() => wsConnection.DisposeAsync()).RunSync()));
            disposable.Add(wsConnection.On(nameof(IWsHubClient.ReceiveMessage), 
                (JObject msg) => DispatchMessage(msg)));
            disposable.Add(wsConnection.On(nameof(IWsHubClient.InvokeRequest), 
                (JObject msg) => HandleRequest(msg)));

            await TryConnect();
        }
        
        private async Task HandleRequest(JObject obj)
        {
            try
            {
                var response = await RequestHandler(Message.MaterializeConcreteMessage(obj));
                response.SenderId = ServiceRegistration.ServiceId;
                response.MessageType = response.GetType().FullName;
                await wsConnection.SendAsync(nameof(IWsHubServer.AcceptResponse), response);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, $"Error handling request {obj}");
                throw;
            }
        }

        private void DispatchMessage(JObject obj)
        {
            try
            {
                var msg = Message.MaterializeConcreteMessage(obj);
                if (lastSeenMessageIds.TryAdd(msg.MessageId, systemClock.UtcNow.DateTime))
                    messages.OnNext(msg);
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
            msg.MessageType = msg.GetType().FullName;
            await SendCore(msg);
        }
        
        public async Task SendToTopic(string topicId, Message msg)
        {
            msg.Target = new MessageTarget
            {
                Type = TargetType.Topic,
                TargetId = topicId
            };
            msg.MessageType = msg.GetType().FullName;
            await SendCore(msg);
        }
        
        public async Task SendCore(Message msg)
        {
            msg.SenderId = ServiceRegistration.ServiceId;
            msg.MessageType = msg.GetType().FullName;
            await wsConnection.InvokeAsync(nameof(IWsHubServer.SendTo), msg);
        }
        
        public async Task<T> InvokeRequest<T>(string targetId, RequestMessage msg)
        {
            msg.SenderId = ServiceRegistration.ServiceId;
            msg.Target = new MessageTarget{Type = TargetType.Direct, TargetId = targetId};
            msg.MessageType = msg.GetType().FullName;
            return await wsConnection.InvokeAsync<T>(nameof(IWsHubServer.InvokeRequest), msg);
        }

        public async Task RegisterService(ServiceFeatures serviceFeatures)
        {
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Register), new RegisterServiceMessage
            {
                Features = serviceFeatures
            });
            logger.Information($"Registered as {ServiceRegistration}");
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
            foreach (var topicId in topicIds)
            {
                topicSubscriptions.TryAdd(topicId, topicId);
            }
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Subscribe),
                new TopicSubscribeMessage
                {
                    TopicIds = topicIds
                });
        }
        
        public async Task Unsubscribe(params string[] topicIds)
        {
            foreach (var topicId in topicIds)
            {
                topicSubscriptions.TryRemove(topicId, out _);
            }
            await wsConnection.InvokeAsync(nameof(IWsHubServer.Unsubscribe),
                new TopicSubscribeMessage
                {
                    TopicIds = topicIds
                });
        }
        
        async Task HandleDisconnect(Exception ex)
        {
            webSocketConnected.OnNext(new WsConnectionStatus{Exception = ex});
            await Task.Delay(options.ReconnectTimeout);
            _ = TryConnect();
        }
        
        async Task TryConnect()
        {
            try
            {
                if (wsConnection.State != HubConnectionState.Connected && wsConnection.State != HubConnectionState.Connecting
                                                                       && !disposed)
                {
                    await wsConnection.StartAsync();
                    logger.Information("TryConnect success");
                    await RegisterService(ServiceRegistration.Features);
                    await Subscribe(topicSubscriptions.Keys.ToArray());
                }
                webSocketConnected.OnNext(new WsConnectionStatus{IsConnected = true});
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
                    .Where(x => (systemClock.UtcNow.DateTime - x.Value) 
                                > options.LastSeenMessageIdsRetentionPeriod).ToList();
                foreach (var oldMessageId in oldMessageIds)
                {
                    lastSeenMessageIds.TryRemove(oldMessageId.Key, out _);
                }
                await Task.Delay(options.LastSeenMessageIdsCleanupPeriod);
            }
        }
        
        public void Dispose()
        {
            disposed = true;
            disposable.DisposeSafe();
        }
    }
}