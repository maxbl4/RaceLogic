using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions.Messages;
using Serilog;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class SubscriptionManager
    {
        private readonly ISubscriptionStorage subscriptionStorage;
        private readonly ICheckpointStorage checkpointStorage;
        private readonly ILogger logger = Log.ForContext<SubscriptionManager>();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly CompositeDisposable disposable;
        private readonly ConcurrentDictionary<string, IDisposable> clients = new ConcurrentDictionary<string, IDisposable>();
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        private readonly SerialDisposable wsClientDisposable;
        private WsHubClient wsClient;

        public SubscriptionManager(ISubscriptionStorage subscriptionStorage, ICheckpointStorage checkpointStorage)
        {
            this.subscriptionStorage = subscriptionStorage;
            this.checkpointStorage = checkpointStorage;
            disposable.Add(wsClientDisposable = new SerialDisposable());
        }

        public async Task OptionsChanged()
        {
            var options = await subscriptionStorage.GetOptions();
            wsClientDisposable.Disposable = wsClient = new WsHubClient(options.ConnectionOptions);
            wsClient.RequestHandler = HandleRequest;
            await wsClient.Connect();
        }

        private Task<Message> HandleRequest(Message arg)
        {
            switch (arg)
            {
                case SubscribeRequest sub:
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

        public void StartStream(string contextConnectionId, in DateTime from)
        {
            try
            {
                rwlock.EnterWriteLock();
                StopStream(contextConnectionId);
                clients[contextConnectionId] = checkpointStorage.ListCheckpoints(from)
                    .ToObservable()
                    .Buffer(TimeSpan.FromMilliseconds(100), 100)
                    .Concat(checkpoints.Select(x => new[] {x}))
                    .Where(x => x.Count > 0)
                    .Select(x =>
                        Observable.FromAsync(() =>
                            logger.Swallow(async () =>
                            {
                                logger.Information($"Sending checkpoint {x} via WS to {contextConnectionId}");
                                await wsClient.InvokeRequest<Message>(contextConnectionId, new ChekpointsUpdate
                                {
                                    Checkpoints = x,
                                });
                            })))
                    .Concat()
                    .Subscribe();
                logger.Information($"Client subscribed {contextConnectionId}");
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