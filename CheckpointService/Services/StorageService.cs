using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.CheckpointService.Model;
using maxbl4.Race.Logic.Checkpoints;
using Microsoft.Extensions.Options;
using ServiceBase;

namespace maxbl4.Race.CheckpointService.Services
{
    public class StorageService : StorageServiceBase
    {
        private readonly IOptions<ServiceOptions> serviceOptions;

        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock): 
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
            this.serviceOptions = serviceOptions;
        }

        protected override void ValidateDatabase()
        {
            repo.Query<Checkpoint>().OrderBy(x => x.Id).FirstOrDefault();
            repo.Query<Tag>().OrderBy(x => x.Id).FirstOrDefault();
        }

        protected override void SetupIndexes()
        {
            repo.Database.GetCollection<Checkpoint>().EnsureIndex(x => x.Timestamp);
            repo.Database.GetCollection<Tag>().EnsureIndex(x => x.DiscoveryTime);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            if (cp == null) throw new ArgumentNullException(nameof(cp));
            repo.Insert(cp);
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var query = repo.Query<Checkpoint>();
            return query.Where(x => (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value))
                .ToList();
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
        
        public void AppendTag(Tag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            repo.Insert(tag);
        }

        public List<Tag> ListTags(DateTime? start = null, DateTime? end = null, int? count = null)
        {
            var query = repo.Query<Tag>();
            return query.Where(x => (start == null || x.DiscoveryTime >= start.Value) && (end == null || x.DiscoveryTime < end.Value))
                .OrderByDescending(x => x.DiscoveryTime)
                .Limit(count ?? int.MaxValue)
                .ToList();
        }
        
        public int DeleteTags(DateTime? start = null, DateTime? end = null)
        {
            return repo.Database.GetCollection<Tag>().DeleteMany(x =>
                (start == null || x.DiscoveryTime >= start.Value) && (end == null || x.DiscoveryTime < end.Value));
        }

        public RfidOptions GetRfidOptions()
        {
            return repo.Query<RfidOptions>().FirstOrDefault()
                ?? serviceOptions.Value.InitialRfidOptions 
                ?? RfidOptions.Default;
        }
        
        public void SetRfidOptions(RfidOptions rfidOptions, bool publishUpdate = true)
        {
            logger.Information($"Persisting RfidOptions {rfidOptions}");
            rfidOptions.Timestamp = systemClock.UtcNow.UtcDateTime; 
            repo.Upsert(rfidOptions);
            logger.Swallow(() => messageHub.Publish(rfidOptions));
        }

        public void UpdateRfidOptions(Action<RfidOptions> modifier)
        {
            var opts = GetRfidOptions();
            modifier(opts);
            SetRfidOptions(opts);
        }
    }
}