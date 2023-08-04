using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using LiteDB;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using maxbl4.Race.Logic.WsHub.Subscriptions.Storage;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.CheckpointService
{
    public class CheckpointRepository : ICheckpointStorage, ISubscriptionStorage, IUpstreamOptionsStorage, IRepository
    {
        private static readonly ILogger logger = Log.ForContext<CheckpointRepository>();
        private readonly IOptions<ServiceOptions> serviceOptions;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;

        public CheckpointRepository(IOptions<ServiceOptions> serviceOptions,
            IStorageService storageService, IMessageHub messageHub, ISystemClock systemClock)
        {
            this.serviceOptions = serviceOptions;
            StorageService = storageService;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            SetupIndexes(storageService.Repo);
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var query = StorageService.Repo.Query<Checkpoint>();
            return query.Where(x =>
                    (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value))
                .ToList();
        }

        public Task AddSubscription(string senderId, DateTime fromTimestamp, DateTime subscriptionExpiration)
        {
            var existing = StorageService.Repo.FirstOrDefault<SubscriptionDto>(x => x.SenderId == senderId)
                           ?? new SubscriptionDto {SenderId = senderId};
            existing.FromTimestamp = fromTimestamp;
            existing.SubscriptionExpiration = subscriptionExpiration;
            StorageService.Repo.Upsert(existing);
            return Task.CompletedTask;
        }

        public Task DeleteSubscription(string senderId)
        {
            StorageService.Repo.DeleteMany<SubscriptionDto>(x => x.SenderId == senderId);
            return Task.CompletedTask;
        }

        public Task<List<SubscriptionDto>> GetSubscriptions()
        {
            return Task.FromResult(StorageService.Repo.Query<SubscriptionDto>().ToList());
        }

        public Task DeleteExpiredSubscriptions()
        {
            StorageService.Repo.DeleteMany<SubscriptionDto>(x => x.SubscriptionExpiration <= systemClock.UtcNow.DateTime);
            return Task.CompletedTask;
        }

        public Task<UpstreamOptions> GetUpstreamOptions()
        {
            var options = StorageService.Repo.SingleOrDefault<UpstreamOptions>(x => true)
                          ?? serviceOptions.Value.InitialUpstreamOptions
                          ?? new UpstreamOptions();
            return Task.FromResult(options);
        }

        public Task SetUpstreamOptions(UpstreamOptions options)
        {
            options.Timestamp = systemClock.UtcNow.UtcDateTime;
            StorageService.Repo.Upsert(options);
            logger.Swallow(() => messageHub.Publish(options));
            return Task.CompletedTask;
        }

        public IStorageService StorageService { get; }

        // void IRepository.ValidateDatabase(ILiteRepository repo)
        // {
        //     repo.Query<Checkpoint>().OrderBy(x => x.Id).FirstOrDefault();
        //     repo.Query<Tag>().OrderBy(x => x.Id).FirstOrDefault();
        // }

        private void SetupIndexes(ILiteRepository repo)
        {
            repo.Database.GetCollection<Checkpoint>().EnsureIndex(x => x.Timestamp);
            repo.Database.GetCollection<Tag>().EnsureIndex(x => x.DiscoveryTime);
            repo.Database.GetCollection<SubscriptionDto>().EnsureIndex(x => x.SenderId);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            if (cp == null) throw new ArgumentNullException(nameof(cp));
            StorageService.Repo.Insert(cp);
        }

        public int DeleteCheckpoint(Id<Checkpoint> id)
        {
            return StorageService.Repo.Delete<Checkpoint>(id) ? 1 : 0;
        }

        public int DeleteCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            return StorageService.Repo.Database.GetCollection<Checkpoint>().DeleteMany(x =>
                (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value));
        }

        public void AppendTag(Tag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            StorageService.Repo.Insert(tag);
        }

        public List<Tag> ListTags(DateTime? start = null, DateTime? end = null, int? count = null)
        {
            var query = StorageService.Repo.Query<Tag>();
            return query.Where(x =>
                    (start == null || x.DiscoveryTime >= start.Value) && (end == null || x.DiscoveryTime < end.Value))
                .OrderByDescending(x => x.DiscoveryTime)
                .Limit(count ?? int.MaxValue)
                .ToList();
        }

        public int DeleteTags(DateTime? start = null, DateTime? end = null)
        {
            return StorageService.Repo.Database.GetCollection<Tag>().DeleteMany(x =>
                (start == null || x.DiscoveryTime >= start.Value) && (end == null || x.DiscoveryTime < end.Value));
        }

        public RfidOptions GetRfidOptions()
        {
            return StorageService.Repo.Query<RfidOptions>().FirstOrDefault()
                   ?? serviceOptions.Value.InitialRfidOptions
                   ?? RfidOptions.Default;
        }

        public void SetRfidOptions(RfidOptions rfidOptions, bool publishUpdate = true)
        {
            logger.Information($"Persisting RfidOptions {rfidOptions}");
            rfidOptions.Timestamp = systemClock.UtcNow.UtcDateTime;
            StorageService.Repo.Upsert(rfidOptions);
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