using System;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using SequentialGuid;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Storage
{
    public class SubscriptionDto : IHasId<SubscriptionDto>
    {
        public string SenderId { get; set; }
        public DateTime FromTimestamp { get; set; }
        public DateTime SubscriptionExpiration { get; set; }
        public Guid Id { get; set; } = SequentialGuidGenerator.Instance.NewGuid();
    }
}