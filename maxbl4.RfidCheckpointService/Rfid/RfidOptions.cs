using System;
using maxbl4.RfidDotNet;

namespace maxbl4.RfidCheckpointService.Rfid
{
    public class RfidOptions
    {
        public static readonly RfidOptions Default = new RfidOptions
        {
            SerializedConnectionString = "Protocol=Alien;Network=127.0.01:30023",
            CheckpointAggregationWindowMs = 200
        };
        
        public string SerializedConnectionString { get; set; }
        public bool RfidEnabled { get; set; }
        public int CheckpointAggregationWindowMs { get; set; }
        
        public ConnectionString GetConnectionString() => ConnectionString.Parse(SerializedConnectionString);
        public void SetConnectionString(ConnectionString connectionString) => SerializedConnectionString = connectionString.ToString();
    }
}