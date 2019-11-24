using System;
using System.Threading.Tasks;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace maxbl4.RfidCheckpointService.Hubs
{
    public class CheckpointsHub : Hub
    {
        private readonly DistributionService distributionService;
        private readonly ILogger logger = Log.ForContext<CheckpointsHub>();

        public CheckpointsHub(DistributionService distributionService)
        {
            this.distributionService = distributionService;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            logger.Information(exception, $"Client {Context.ConnectionId} disconnected");
            distributionService.StopStream(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Subscribe(DateTime from)
        {
            logger.Information( $"Subscribe request {Context.ConnectionId}");
            distributionService.StartStream(Context.ConnectionId, from);
        }
    }
}