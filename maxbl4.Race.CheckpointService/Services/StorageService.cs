using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.CheckpointService.Model;
using maxbl4.Race.Logic.Checkpoints;
using Microsoft.Extensions.Options;
using Serilog;
using ConnectionString = LiteDB.ConnectionString;

namespace maxbl4.Race.CheckpointService.Services
{
    public class StorageService : IDisposable
    {
        private readonly IOptions<ServiceOptions> serviceOptions;
        private readonly IMessageHub messageHub;
        private readonly ILogger logger = Log.ForContext<StorageService>();
        private readonly ISystemClock systemClock;
        private LiteRepository repo;

        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock)
        {
            this.serviceOptions = serviceOptions;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            var cs = new ConnectionString(serviceOptions.Value.StorageConnectionString){UtcDate = true};
            logger.SwallowError(() => Initialize(cs), ex =>
            {
                cs = TryRotateDatabase(cs);
                Initialize(cs);
            });
        }

        private void Initialize(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).CurrentFile;
            logger.Information($"Using storage file {connectionString.Filename}");
            repo = new LiteRepository(connectionString);
            SetupIndexes();
            ValidateDatabase();
        }

        private ConnectionString TryRotateDatabase(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).NextFile;
            return connectionString;
        }

        private void ValidateDatabase()
        {
            repo.Query<Checkpoint>().OrderBy(x => x.Id).FirstOrDefault();
            repo.Query<Tag>().OrderBy(x => x.Id).FirstOrDefault();
        }

        private void SetupIndexes()
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

        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}