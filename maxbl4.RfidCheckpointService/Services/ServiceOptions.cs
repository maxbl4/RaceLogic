using LiteDB;

namespace maxbl4.RfidCheckpointService.Services
{
    public class ServiceOptions
    {
        public string StorageConnectionString { get; set; }
        public int PauseInStartupMs { get; set; }
        public RfidOptions InitialRfidOptions { get; set; }
    }
}