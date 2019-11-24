using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using System.Threading;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RaceLogic.Checkpoints;
using Serilog;
using Microsoft.Extensions.Options;

namespace maxbl4.RfidCheckpointService.Services
{
    public class StorageService : IDisposable
    {
        private readonly IMessageHub messageHub;
        private readonly ILogger logger = Log.ForContext<StorageService>();
        private readonly ISystemClock systemClock;
        private readonly LiteRepository repo;
        private long checkpointId;

        public StorageService(IOptions<ServiceOptions> options, IMessageHub messageHub, ISystemClock systemClock)
        {
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            logger.Information($"Using storage connection string {options.Value?.StorageConnectionString}");
            var connectionString = new ConnectionString(options.Value.StorageConnectionString) {UtcDate = true};
            repo = new LiteRepository(connectionString);
            SetupIndexes();
            checkpointId = GetLastCheckpointId();
        }

        private void SetupIndexes()
        {
            repo.Database.GetCollection<Checkpoint>().EnsureIndex(x => x.Timestamp);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            cp.Id = NextCheckpointId();
            repo.Insert(cp);
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var query = repo.Query<Checkpoint>();
            return query.Where(x => (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value)).ToList();
        }

        public int DeleteCheckpoint(long id)
        {
            return repo.Delete<Checkpoint>(id) ? 1 : 0;
        }
        
        public int DeleteCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            return repo.Database.GetCollection<Checkpoint>().DeleteMany(x =>
                (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value));
        }

        public RfidOptions GetRfidOptions()
        {
            return repo.Query<RfidOptions>().FirstOrDefault() ?? RfidOptions.Default;
        }
        
        public void SetRfidOptions(RfidOptions rfidOptions, bool publishUpdate = true)
        {
            logger.Information($"Persisting RfidOptions {rfidOptions}");
            rfidOptions.Timestamp = systemClock.UtcNow.UtcDateTime; 
            repo.Upsert(rfidOptions);
            logger.SwallowError(() => messageHub.Publish(rfidOptions));
        }

        public void UpdateRfidOptions(Action<RfidOptions> modifier)
        {
            var opts = GetRfidOptions();
            modifier(opts);
            SetRfidOptions(opts);
        }

        private long NextCheckpointId()
        {
            return Interlocked.Increment(ref checkpointId);
        }

        private long GetLastCheckpointId()
        {
            var lastCheckpoint = repo.Query<Checkpoint>().OrderByDescending(x => x.Id).FirstOrDefault();
            return lastCheckpoint?.Id ?? 0;
        }

        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}