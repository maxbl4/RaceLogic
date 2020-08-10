using System;

namespace maxbl4.Race.Services.CheckpointService.Model
{
    public class ReaderStatus
    {
        public bool IsConnected { get; set; }
        public DateTime Heartbeat { get; set; }
    }
}