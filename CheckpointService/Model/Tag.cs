using System;
using maxbl4.Race.Logic;

namespace maxbl4.Race.CheckpointService.Model
{
    public class Tag
    {
        public long Id { get; set; }
        public string TagId { get; set; }
        public DateTime DiscoveryTime { get; set; } = Constants.DefaultUtcDate;
        public DateTime LastSeenTime { get; set; } = Constants.DefaultUtcDate;
        public int Antenna { get; set; }
        public int ReadCount { get; set; }
        public decimal Rssi { get; set; }
    }
}