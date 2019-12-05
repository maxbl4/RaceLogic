using System;

namespace maxbl4.Race.CheckpointService.Model
{
    public class ReaderStatus
    {
        public bool IsConnected { get; set; }
        public DateTime Heartbeat { get; set; }
    }
}