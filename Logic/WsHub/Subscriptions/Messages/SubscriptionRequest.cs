using System;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Messages
{
    public class SubscriptionRequest: RequestMessage
    {
        public SubscriptionRequestTypes RequestType { get; set; } = SubscriptionRequestTypes.Subscribe;
        public DateTime FromTimestamp { get; set; }
        public DateTime SubscriptionExpiration { get; set; } = DateTime.UtcNow.AddDays(1);
    }

    public enum SubscriptionRequestTypes
    {
        Subscribe,
        Unsubscribe
    }
}