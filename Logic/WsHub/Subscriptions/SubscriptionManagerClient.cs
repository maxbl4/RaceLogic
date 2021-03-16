using System;
using System.Threading.Tasks;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class SubscriptionManagerClient : IAsyncInitialize, IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return default;
        }

        public Task InitializeAsync()
        {
            return null;
        }
    }
}