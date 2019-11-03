using System;
using Microsoft.AspNetCore.SignalR;

namespace maxbl4.RfidCheckpointService.Hubs
{
    public class CheckpointsHub : Hub
    {
        public void Subscribe(DateTime from)
        {
        }
    }
}