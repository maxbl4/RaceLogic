using System;
using System.Collections.Generic;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.RaceLogic.Checkpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace maxbl4.RfidCheckpointService.Services
{
    public class StorageService : IDisposable
    {
        private readonly IMessageHub messageHub;
        private readonly ILogger<StorageService> logger;
        private readonly LiteRepository repo;
        private long checkpointId = -1;

        public StorageService(IOptions<ServiceOptions> options, IMessageHub messageHub, ILogger<StorageService> logger)
        {
            this.messageHub = messageHub;
            this.logger = logger;
            logger.LogInformation($"Using storage connection string {options.Value?.StorageConnectionString}");
            var connectionString = new ConnectionString(options.Value.StorageConnectionString) {UtcDate = true};
            repo = new LiteRepository(connectionString);
            SetupIndexes();
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
        
        public void SetRfidOptions(RfidOptions rfidOptions)
        {
            logger.LogInformation($"Persisting RfidOptions {rfidOptions}");
            repo.Upsert(rfidOptions);
            Safe.Execute(() => messageHub.Publish(rfidOptions), logger);
        }

        public void UpdateRfidOptions(Action<RfidOptions> modifier)
        {
            var opts = GetRfidOptions();
            modifier(opts);
            SetRfidOptions(opts);
        }

        private long NextCheckpointId()
        {
            if (checkpointId < 0)
            {
                var lastCheckpoint = repo.Query<Checkpoint>().OrderByDescending(x => x.Id).FirstOrDefault();
                checkpointId = lastCheckpoint?.Id ?? 0;
            }

            checkpointId++;
            return checkpointId;
        }

        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}