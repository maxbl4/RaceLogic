using System;
using System.Collections.Generic;
using Easy.MessageHub;
using LiteDB;
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
            repo.Insert(cp);
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var query = repo.Query<Checkpoint>();
            return query.Where(x => (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value)).ToList();
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

        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}