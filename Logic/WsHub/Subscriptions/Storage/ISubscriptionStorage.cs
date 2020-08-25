using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Storage
{
    public interface ISubscriptionStorage
    {
        Task<SubscriptionManagerOptions> GetSubscriptionManagerOptions();
        Task AddSubscription(string senderId, DateTime fromTimestamp, DateTime subscriptionExpiration);
        Task<List<SubscriptionDto>> GetSubscriptions();
        Task SetSubscriptionManagerOptions(SubscriptionManagerOptions options);
        Task DeleteSubscription(string senderId);
        Task DeleteExpiredSubscriptions();
    }
}