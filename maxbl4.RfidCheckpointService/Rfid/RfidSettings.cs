using System;
using maxbl4.RfidDotNet;

namespace maxbl4.RfidCheckpointService.Rfid
{
    public class RfidSettings
    {
        public static readonly RfidSettings Default = new RfidSettings
        {
            SerializedConnectionString = "Protocol=Fake",
            CheckpointAggregationWindow = TimeSpan.FromMilliseconds(200)
        };
        
        public string SerializedConnectionString { get; set; }
        public bool RfidEnabled { get; set; }
        public TimeSpan CheckpointAggregationWindow { get; set; }
        
        public ConnectionString GetConnectionString() => ConnectionString.Parse(SerializedConnectionString);
        public void SetConnectionString(ConnectionString connectionString) => SerializedConnectionString = connectionString.ToString();
    }
}