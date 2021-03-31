using System;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using maxbl4.Race.Logic.WsHub.Subscriptions.Storage;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.CheckpointService.Services
{
    public class StorageService : StorageServiceBase, ICheckpointStorage, ISubscriptionStorage, IUpstreamOptionsStorage
    {
        private readonly IOptions<ServiceOptions> serviceOptions;

        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock) :
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
            this.serviceOptions = serviceOptions;
        }

        public List<Checkpoint> ListCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var query = repo.Query<Checkpoint>();
            return query.Where(x =>
                    (start == null || x.Timestamp >= start.Value) && (end == null || x.Timestamp < end.Value))
                .ToList();
        }

        public Task AddSubscription(string senderId, DateTime fromTimestamp, DateTime subscriptionExpiration)
        {
            var existing = repo.FirstOrDefault<SubscriptionDto>(x => x.SenderId == senderId)
                           ?? new SubscriptionDto {SenderId = senderId};
            existing.FromTimestamp = fromTimestamp;
            existing.SubscriptionExpiration = subscriptionExpiration;
            repo.Upsert(existing);
            return Task.CompletedTask;
        }

        public Task DeleteSubscription(string senderId)
        {
            repo.DeleteMany<SubscriptionDto>(x => x.SenderId == senderId);
            return Task.CompletedTask;
        }

        public Task<List<SubscriptionDto>> GetSubscriptions()
        {
            return Task.FromResult(repo.Query<SubscriptionDto>().ToList());
        }

        public Task DeleteExpiredSubscriptions()
        {
            repo.DeleteMany<SubscriptionDto>(x => x.SubscriptionExpiration <= systemClock.UtcNow.DateTime);
            return Task.CompletedTask;
        }

        public Task<UpstreamOptions> GetUpstreamOptions()
        {
            var options = repo.SingleOrDefault<UpstreamOptions>(x => true)
                          ?? serviceOptions.Value.InitialUpstreamOptions
                          ?? new UpstreamOptions();
            return Task.FromResult(options);
        }

        public Task SetUpstreamOptions(UpstreamOptions options)
        {
            options.Timestamp = systemClock.UtcNow.UtcDateTime;
            repo.Upsert(options);
            logger.Swallow(() => messageHub.Publish(options));
            return Task.CompletedTask;
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
            repo.Database.GetCollection<SubscriptionDto>().EnsureIndex(x => x.SenderId);
        }

        public void AppendCheckpoint(Checkpoint cp)
        {
            if (cp == null) throw new ArgumentNullException(nameof(cp));
            repo.Insert(cp);
        }

        public int DeleteCheckpoint(Id<Checkpoint> id)
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
            return query.Where(x =>
                    (start == null || x.DiscoveryTime >= start.Value) && (end == null || x.DiscoveryTime < end.Value))
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