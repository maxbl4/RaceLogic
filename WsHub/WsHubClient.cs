using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.Extensions.TaskExt;
using maxbl4.Race.Logic.CheckpointService.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace maxbl4.Race.WsHub
{
    public class WsHubClient: IDisposable
    {
        private volatile bool disposed = false;
        private readonly ILogger logger = Log.ForContext<WsHubClient>();
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly string address;
        private HubConnection wsConnection;
        private readonly BehaviorSubject<WsConnectionStatus> webSocketConnected = new BehaviorSubject<WsConnectionStatus>(new WsConnectionStatus());
        public IObservable<WsConnectionStatus> WebSocketConnected => webSocketConnected;
        
        private readonly Subject<MessageBase> messages = new Subject<MessageBase>();
        private readonly TimeSpan reconnectTimeout;
        public IObservable<MessageBase> Messages => messages;

        public string ClientId { get; }

        public WsHubClient(string address, string clientId, TimeSpan? reconnectTimeout = null)
        {
            if (!address.EndsWith("/"))
                address += "/";
            this.address = address;
            this.ClientId = clientId;
            this.reconnectTimeout = reconnectTimeout ?? TimeSpan.FromMilliseconds(5000);
        }

        public async Task Connect()
        {
            wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{address}ws/hub", options =>
                {
                    options.Credentials = new NetworkCredential(ClientId, "password");
                })
                .Build();
            wsConnection.Closed += exception =>
            {
                logger.Information("Connection closed");
                _ = TryConnect();
                return Task.CompletedTask;
            };
            
            disposable.Add(Disposable.Create(() => logger.Swallow(() => wsConnection.DisposeAsync()).RunSync()));
            disposable.Add(wsConnection.On(nameof(IWsHubClient.ReceiveMessage), (MessageBase msg) => messages.OnNext(msg)));

            await TryConnect();
        }

        public async Task SendTo(string targetId, MessageBase msg)
        {
            msg.SenderId = this.ClientId;
            msg.TargetId = targetId;
            await wsConnection.SendAsync(nameof(WsHub.SendTo), msg);
        }
        
        async Task HandleDisconnect(Exception ex)
        {
            webSocketConnected.OnNext(new WsConnectionStatus{Exception = ex});
            await Task.Delay(reconnectTimeout);
            _ = TryConnect();
        }
        
        async Task TryConnect()
        {
            try
            {
                logger.Warning("TryConnect");
                if (wsConnection.State != HubConnectionState.Connected && wsConnection.State != HubConnectionState.Connecting
                                                                       && !disposed)
                {
                    await wsConnection.StartAsync();
                    logger.Warning("TryConnect success");
                }
                webSocketConnected.OnNext(new WsConnectionStatus{IsConnected = true});
            }
            catch (Exception ex)
            {
                logger.Warning("TryConnect failed", ex);
                HandleDisconnect(ex).Wait(0);
            }
        }
        
        public void Dispose()
        {
            disposed = true;
            disposable.DisposeSafe();
        }
    }
}