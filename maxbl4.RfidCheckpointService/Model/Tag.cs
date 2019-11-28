using System;

namespace maxbl4.RfidCheckpointService.Model
{
    public class Tag
    {
        public long Id { get; set; }
        public string TagId { get; set; }
        public DateTime DiscoveryTime { get; set; } = new DateTime(0L, DateTimeKind.Utc);
        public DateTime LastSeenTime { get; set; } = new DateTime(0L, DateTimeKind.Utc);
        public int Antenna { get; set; }
        public int ReadCount { get; set; }
        public decimal Rssi { get; set; }
    }
}