using maxbl4.RfidDotNet;
using Newtonsoft.Json;

namespace maxbl4.RfidCheckpointService.Services
{
    public class RfidOptions
    {
        public static readonly RfidOptions Default = new RfidOptions
        {
            ConnectionString = "Protocol=Alien;Network=127.0.0.1:20023",
            CheckpointAggregationWindowMs = 200
        };

        public int Id => 1;
        
        /// <summary>
        /// RfidDotnet connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Is Rfid reading enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Windows in milliseconds for tag deduplication
        /// </summary>
        public int CheckpointAggregationWindowMs { get; set; }
        /// <summary>
        /// Reads per second threshold below which a tag would be reported as bad 
        /// </summary>
        public int RpsThreshold { get; set; }
        
        public ConnectionString GetConnectionString() => RfidDotNet.ConnectionString.Parse(ConnectionString);
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}