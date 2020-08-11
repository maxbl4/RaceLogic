using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoMapper.Internal;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.Extensions.TaskExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public class CheckpointSubscription: ICheckpointSubscription
    {
        private volatile bool disposed = false;
        private readonly ILogger logger = Log.ForContext<CheckpointServiceClient>();
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private HubConnection wsConnection;
        
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        public IObservable<Checkpoint> Checkpoints => checkpoints;
        
        private readonly BehaviorSubject<ReaderStatus> readerStatus = new BehaviorSubject<ReaderStatus>(new ReaderStatus());
        public IObservable<ReaderStatus> ReaderStatus => readerStatus;
        
        private readonly BehaviorSubject<WsConnectionStatus> webSocketConnected = new BehaviorSubject<WsConnectionStatus>(new WsConnectionStatus());
        private readonly string address;
        private readonly DateTime @from;
        private readonly TimeSpan reconnectTimeout;
        public IObservable<WsConnectionStatus> WebSocketConnected => webSocketConnected;

        public CheckpointSubscription(string address, DateTime from, TimeSpan? reconnectTimeout)
        {
            if (!address.EndsWith("/"))
                address += "/";
            this.address = address;
            this.@from = @from;
            this.reconnectTimeout = reconnectTimeout ?? TimeSpan.FromMilliseconds(5000);
            disposable.Add(checkpoints);
            disposable.Add(readerStatus);
            disposable.Add(webSocketConnected);
        }
        
        public void Start()
        {   
            logger.Information("Connect");

            wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{address}ws/cp")
                .Build();
            wsConnection.Closed += exception =>
            {
                logger.Information("Connection closed");
                _ = TryConnect();
                return Task.CompletedTask;
            };

            disposable.Add(Disposable.Create(() => logger.Swallow(() => wsConnection.DisposeAsync()).RunSync()));
            disposable.Add(wsConnection.On("Checkpoint", (Checkpoint[] cps) => cps.ForAll(cp => checkpoints.OnNext(cp))));
            disposable.Add(wsConnection.On("ReaderStatus", (ReaderStatus s) => readerStatus.OnNext(s)));

            _ = TryConnect();
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
                    await Subscribe(from);
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
        
        private async Task Subscribe(DateTime from)
        {
            logger.Information("Subscribe {from}", from);
            await wsConnection.SendCoreAsync("Subscribe", new object[]{from});
        }

        public void Dispose()
        {
            disposed = true;
            disposable.DisposeSafe();
        }
    }
}