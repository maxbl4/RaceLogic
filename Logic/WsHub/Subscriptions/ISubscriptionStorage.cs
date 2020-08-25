using System.Threading.Tasks;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public interface ISubscriptionStorage
    {
        Task<SubscriptionManagerOptions> GetOptions();
    }
}