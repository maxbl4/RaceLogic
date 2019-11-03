using System;
using maxbl4.RfidCheckpointService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace maxbl4.RfidCheckpointService.Services
{
    public class DistributionService : IDisposable
    {
        private readonly IHubContext<CheckpointsHub> checkpointsHub;

        public DistributionService(IHubContext<CheckpointsHub> checkpointsHub)
        {
            this.checkpointsHub = checkpointsHub;
        }

        public void Dispose()
        {
        }
    }
}