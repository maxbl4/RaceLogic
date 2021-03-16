using System.Threading;
using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Race.CheckpointService.Services
{
    public class SubscriptionService : IHostedService
    {
        private readonly SubscriptionManager subscriptionManager;

        public SubscriptionService(SubscriptionManager subscriptionManager)
        {
            this.subscriptionManager = subscriptionManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await subscriptionManager.InitializeAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await subscriptionManager.DisposeAsync();
        }
    }
}