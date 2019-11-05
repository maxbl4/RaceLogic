using System;
using System.Threading.Tasks;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace maxbl4.RfidCheckpointService.Hubs
{
    public class CheckpointsHub : Hub
    {
        private readonly DistributionService distributionService;
        private readonly ILogger<CheckpointsHub> logger;

        public CheckpointsHub(DistributionService distributionService, ILogger<CheckpointsHub> logger)
        {
            this.distributionService = distributionService;
            this.logger = logger;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogInformation(exception, $"Client {Context.ConnectionId} disconnected");
            distributionService.StopStream(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Subscribe(DateTime from)
        {
            logger.LogInformation( $"Subscribe request {Context.ConnectionId}");
            distributionService.StartStream(Context.ConnectionId, from);
        }
    }
}