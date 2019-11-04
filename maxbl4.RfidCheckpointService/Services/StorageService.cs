using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using maxbl4.RaceLogic.Checkpoints;
using Microsoft.Extensions.Options;

namespace maxbl4.RfidCheckpointService.Services
{
    public class StorageService
    {
        private readonly ConnectionString connectionString; 

        public StorageService(IOptions<StorageOptions> options)
        {
            this.connectionString = new ConnectionString(options.Value.ConnectionString) {UtcDate = true};
            SetupIndexes();
        }

        private void SetupIndexes()
        {
            using var repo = new LiteRepository(connectionString);
            repo.Database.GetCollection<Checkpoint>().EnsureIndex(x => x.Timestamp);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            using var repo = new LiteRepository(connectionString);
            repo.Insert(cp);
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            using var repo = new LiteRepository(connectionString);
            var query = repo.Query<Checkpoint>();
            return query.Where(x => (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value)).ToList();
        }

        public RfidOptions GetRfidOptions()
        {
            using var repo = new LiteRepository(connectionString);
            return repo.Query<RfidOptions>().FirstOrDefault() ?? RfidOptions.Default;
        }
        
        public void SetRfidSettings(RfidOptions rfidOptions)
        {
            using var repo = new LiteRepository(connectionString);
            repo.Upsert(rfidOptions);
        }
    }
}