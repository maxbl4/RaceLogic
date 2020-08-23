using System;
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
        
        private readonly Dictionary<Id<Message>, DateTime> lastSeenMessageIds = new Dictionary<Id<Message>, DateTime>();
        private readonly Subject<Message> messages = new Subject<Message>();
        public IObservable<Message> Messages => messages;

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

            await TryConnect();
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
                logger.Warning(ex, "Error dispatching message");
            }
        }

        public async Task SendTo(string targetId, Message msg)
        {
            msg.SenderId = ServiceRegistration.ServiceId;
            msg.TargetId = targetId;
            msg.MessageType = msg.GetType().FullName;
            await wsConnection.SendAsync(nameof(IWsHubServer.SendTo), msg);
        }

        public async Task RegisterService(ServiceFeatures serviceFeatures)
        {
            await wsConnection.SendAsync(nameof(IWsHubServer.Register), new RegisterServiceMessage
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
                    lastSeenMessageIds.Remove(oldMessageId.Key);
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