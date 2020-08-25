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
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions.Storage;
using Serilog;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class SubscriptionManager
    {
        private readonly ISubscriptionStorage subscriptionStorage;
        private readonly ICheckpointStorage checkpointStorage;
        private readonly ISystemClock systemClock;
        private readonly ILogger logger = Log.ForContext<SubscriptionManager>();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly CompositeDisposable disposable;
        private readonly ConcurrentDictionary<string, IDisposable> clients = new ConcurrentDictionary<string, IDisposable>();
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        private readonly SerialDisposable wsClientDisposable;
        private WsHubClient wsClient;

        public SubscriptionManager(ISubscriptionStorage subscriptionStorage, ICheckpointStorage checkpointStorage,
            ISystemClock systemClock = null)
        {
            this.subscriptionStorage = subscriptionStorage;
            this.checkpointStorage = checkpointStorage;
            this.systemClock = systemClock ?? new DefaultSystemClock();
            disposable.Add(wsClientDisposable = new SerialDisposable());
        }

        public async Task InitializeAsync()
        {
            await OptionsChanged();
            await subscriptionStorage.DeleteExpiredSubscriptions();
            var subs = await subscriptionStorage.GetSubscriptions();
            foreach (var sub in subs.Where(x => x.SubscriptionExpiration > systemClock.UtcNow.DateTime))
            {
                StartStream(sub.SenderId, sub.FromTimestamp);
            }
        }

        public async Task OptionsChanged()
        {
            var options = await subscriptionStorage.GetSubscriptionManagerOptions();
            wsClientDisposable.Disposable = wsClient = new WsHubClient(options.ConnectionOptions);
            wsClient.RequestHandler = HandleRequest;
            await wsClient.Connect();
        }

        private async Task<Message> HandleRequest(Message arg)
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
                    await wsClient.InvokeRequest<Message>(client, msg);
                });
            }
        }

        private async Task OnRfidOptions(RfidOptions rfidOptions)
        {
            await BroadcastUpdate(new ChekpointsUpdate
            {
                RfidOptions = rfidOptions
            });
        }
        
        private async Task OnReaderStatus(ReaderStatus readerStatus)
        {
            await BroadcastUpdate(new ChekpointsUpdate
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

        public void Dispose()
        {
            disposable.DisposeSafe();
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
                                await wsClient.InvokeRequest<Message>(targetId, new ChekpointsUpdate
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
    }
}