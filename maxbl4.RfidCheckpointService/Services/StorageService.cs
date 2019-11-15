using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidDotNet.Ext;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace maxbl4.RfidCheckpointService.Services
{
    public class StorageService : IDisposable
    {
        private readonly ILogger<StorageService> logger;
        private readonly LiteRepository repo;

        public StorageService(IOptions<ServiceOptions> options, ILogger<StorageService> logger)
        {
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
        }

        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}