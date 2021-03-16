using System;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Storage
{
    public class SubscriptionDto : IHasId<SubscriptionDto>
    {
        public string SenderId { get; set; }
        public DateTime FromTimestamp { get; set; }
        public DateTime SubscriptionExpiration { get; set; }
        public Id<SubscriptionDto> Id { get; set; } = Id<SubscriptionDto>.NewId();
    }
}