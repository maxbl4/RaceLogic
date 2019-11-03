using System;
using System.Threading.Tasks;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.SignalR;

namespace maxbl4.RfidCheckpointService.Hubs
{
    public class CheckpointsHub : Hub
    {
        private readonly DistributionService distributionService;

        public CheckpointsHub(DistributionService distributionService)
        {
            this.distributionService = distributionService;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            distributionService.StopStream(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Subscribe(DateTime from)
        {
            distributionService.StartStream(Context.ConnectionId, from);
        }
    }
}