using System;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public class WsConnectionStatus
    {
        public bool IsConnected { get; set; }
        public Exception Exception { get; set; }
    }
}