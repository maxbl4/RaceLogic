using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Easy.MessageHub;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Ext;
using maxbl4.RfidCheckpointService.Hubs;
using maxbl4.RfidCheckpointService.Model;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace maxbl4.RfidCheckpointService.Services
{
    public class DistributionService : IDisposable
    {
        private readonly IHubContext<CheckpointsHub> checkpointsHub;
        private readonly StorageService storageService;
        private readonly ILogger logger = Log.ForContext<DistributionService>();
        private readonly CompositeDisposable disposable;
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<string, IDisposable> clients = new Dictionary<string, IDisposable>();
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();

        public DistributionService(IHubContext<CheckpointsHub> checkpointsHub, IMessageHub messageHub, StorageService storageService)
        {
            this.checkpointsHub = checkpointsHub;
            this.storageService = storageService;
            disposable = new CompositeDisposable(messageHub.SubscribeDisposable<Checkpoint>(OnCheckpoint),
                messageHub.SubscribeDisposable<ReaderStatus>(OnReaderStatus));
        }

        private void OnReaderStatus(ReaderStatus readerStatus)
        {
            logger.Swallow(async () =>
            {
                logger.Information($"Broadcasting reader status {readerStatus}");
                await checkpointsHub.Clients.All
                    .SendCoreAsync("ReaderStatus", new[] {readerStatus});
            }).Wait(0);

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
                if (clients.TryGetValue(contextConnectionId, out var d))
                {
                    clients.Remove(contextConnectionId);
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

        public void StartStream(string contextConnectionId, in DateTime @from)
        {
            try
            {
                rwlock.EnterWriteLock();
                StopStream(contextConnectionId);
                clients[contextConnectionId] = storageService.ListCheckpoints(from)
                    .ToObservable()
                    .Buffer(TimeSpan.FromMilliseconds(100), 100)
                    .Concat(checkpoints.Select(x => new []{x}))
                    .Select(x => 
                        Observable.FromAsync(() => 
                            logger.Swallow(async () =>
                            {
                                logger.Information($"Sending checkpoint {x} via WS to {contextConnectionId}");
                                await checkpointsHub.Clients.Client(contextConnectionId)
                                    .SendCoreAsync("Checkpoint", new[] {x});
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