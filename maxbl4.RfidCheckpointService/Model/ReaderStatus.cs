using System;

namespace maxbl4.RfidCheckpointService.Model
{
    public class ReaderStatus
    {
        public bool IsConnected { get; set; }
        public DateTime Heartbeat { get; set; }
    }
}