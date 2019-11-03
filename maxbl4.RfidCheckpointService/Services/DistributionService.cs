using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Easy.MessageHub;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Ext;
using maxbl4.RfidCheckpointService.Hubs;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.Ext;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace maxbl4.RfidCheckpointService.Services
{
    public class DistributionService : IDisposable
    {
        private readonly IHubContext<CheckpointsHub> checkpointsHub;
        private readonly StorageService storageService;
        private readonly ILogger<DistributionService> logger;
        private readonly CompositeDisposable disposable;
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, IDisposable> clients = new Dictionary<string, IDisposable>();
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();

        public DistributionService(IHubContext<CheckpointsHub> checkpointsHub, IMessageHub messageHub, StorageService storageService, ILogger<DistributionService> logger)
        {
            this.checkpointsHub = checkpointsHub;
            this.storageService = storageService;
            this.logger = logger;
            disposable = new CompositeDisposable(messageHub.SubscribeDisposable<Checkpoint>(OnCheckpoint));
        }

        void OnCheckpoint(Checkpoint tag)
        {
            try
            {
                rwlock.EnterReadLock();
                checkpoints.OnNext(tag);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error sending tag to clients");
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
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to stop the stream");
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
                    .Concat(checkpoints)
                    .Select(x => 
                        Observable.FromAsync(() => 
                            Safe.Execute(() => 
                                checkpointsHub.Clients.Client(contextConnectionId)
                                    .SendCoreAsync("Checkpoint", new []{x}), logger)))
                    .Concat()
                    .Subscribe();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to start the stream");
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }
    }
}