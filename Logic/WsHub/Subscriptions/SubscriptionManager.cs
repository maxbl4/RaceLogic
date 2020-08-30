using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.Extensions.MessageHubExt;
using maxbl4.Infrastructure.Extensions.TaskExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions.Storage;
using Serilog;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class SubscriptionManager: IAsyncInitialize, IAsyncDisposable
    {
        private readonly ISubscriptionStorage subscriptionStorage;
        private readonly IUpstreamOptionsStorage upstreamOptionsStorage;
        private readonly ICheckpointStorage checkpointStorage;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;
        private readonly ILogger logger = Log.ForContext<SubscriptionManager>();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly CompositeDisposable disposable;
        private readonly ConcurrentDictionary<string, IDisposable> clients = new ConcurrentDictionary<string, IDisposable>();
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        private WsHubConnection wsConnection;

        public SubscriptionManager(ISubscriptionStorage subscriptionStorage,
            IUpstreamOptionsStorage upstreamOptionsStorage,
            ICheckpointStorage checkpointStorage, 
            IMessageHub messageHub,
            ISystemClock systemClock = null)
        {
            this.subscriptionStorage = subscriptionStorage;
            this.upstreamOptionsStorage = upstreamOptionsStorage;
            this.checkpointStorage = checkpointStorage;
            this.messageHub = messageHub;
            this.systemClock = systemClock ?? new DefaultSystemClock();
            disposable = new CompositeDisposable(
                messageHub.Subscribe<UpstreamOptions>(OptionsChanged),
                messageHub.Subscribe<Checkpoint>(OnCheckpoint),
                messageHub.Subscribe<RfidOptions>(OnRfidOptions),
                messageHub.Subscribe<ReaderStatus>(OnReaderStatus));
        }

        public async Task InitializeAsync()
        {
            OptionsChanged(await upstreamOptionsStorage.GetUpstreamOptions());
            await subscriptionStorage.DeleteExpiredSubscriptions();
            var subs = await subscriptionStorage.GetSubscriptions();
            foreach (var sub in subs.Where(x => x.SubscriptionExpiration > systemClock.UtcNow.DateTime))
            {
                StartStream(sub.SenderId, sub.FromTimestamp);
            }
        }

        private void OptionsChanged(UpstreamOptions options)
        {
            options.ConnectionOptions.Features |= ServiceFeatures.CheckpointService;
            if (wsConnection != null)
                logger.Swallow(async () => await wsConnection.DisposeAsync()).RunSync();
            wsConnection = new WsHubConnection(options.ConnectionOptions);
            wsConnection.RegisterRequestHandler<SubscriptionRequest>(HandleRequest);
            _ = wsConnection.Connect();
        }

        private async Task<Message> HandleRequest(SubscriptionRequest arg)
        {
            switch (arg)
            {
                case SubscriptionRequest sub:
                    switch (sub.RequestType)
                    {
                        case SubscriptionRequestTypes.Subscribe:
                            await subscriptionStorage.AddSubscription(sub.SenderId, sub.FromTimestamp, sub.SubscriptionExpiration);
                            StartStream(sub.SenderId, sub.FromTimestamp);
                            return new SubscriptionResponse();
                        case SubscriptionRequestTypes.Unsubscribe:
                            await subscriptionStorage.DeleteSubscription(sub.SenderId);
                            StopStream(sub.SenderId);
                            return new SubscriptionResponse();
                    }
                    break;
            }
            return null;
        }
        
        private async Task BroadcastUpdate(ChekpointsUpdate msg, [CallerMemberName]string caller = null)
        {
            foreach (var client in clients.Keys.ToList())
            {
                await logger.Swallow(async () =>
                {
                    logger.Information(caller);
                    await wsConnection.InvokeRequest<ChekpointsUpdate, Message>(client, msg);
                });
            }
        }

        private void OnRfidOptions(RfidOptions rfidOptions)
        {
            _ = BroadcastUpdate(new ChekpointsUpdate
            {
                RfidOptions = rfidOptions
            });
        }
        
        private void OnReaderStatus(ReaderStatus readerStatus)
        {
            _ = BroadcastUpdate(new ChekpointsUpdate
            {
                ReaderStatus = readerStatus
            });
        }

        void OnCheckpoint(Checkpoint checkpoint)
        {
            try
            {
                rwlock.EnterReadLock();
                logger.Information($"Received checkpoint: {checkpoint}");
                checkpoints.OnNext(checkpoint);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error sending tag to clients");
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        public void StopStream(string contextConnectionId)
        {
            try
            {
                rwlock.EnterWriteLock();
                if (clients.TryRemove(contextConnectionId, out var d))
                {
                    d.DisposeSafe();
                    logger.Information($"Client unsubscribed {contextConnectionId}");
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to stop the stream");
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public void StartStream(string targetId, in DateTime from)
        {
            try
            {
                rwlock.EnterWriteLock();
                StopStream(targetId);
                clients[targetId] = checkpointStorage.ListCheckpoints(from)
                    .ToObservable()
                    .Buffer(TimeSpan.FromMilliseconds(100), 100)
                    .Concat(checkpoints.Select(x => new[] {x}))
                    .Where(x => x.Count > 0)
                    .Select(x =>
                        Observable.FromAsync(() =>
                            logger.Swallow(async () =>
                            {
                                logger.Information($"Sending checkpoint {x} via WS to {targetId}");
                                await wsConnection.InvokeRequest<ChekpointsUpdate, Message>(targetId, new ChekpointsUpdate
                                {
                                    Checkpoints = x,
                                });
                            })))
                    .Concat()
                    .Subscribe();
                logger.Information($"Client subscribed {targetId}");
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to start the stream");
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public async ValueTask DisposeAsync()
        {
            disposable.DisposeSafe();
            await logger.Swallow(async () => await wsConnection.DisposeAsync());
        }
    }
}