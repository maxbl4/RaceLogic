using maxbl4.Race.Services.CheckpointService.Model;

namespace maxbl4.Race.CheckpointService.Services
{
    public class ServiceOptions
    {
        public string StorageConnectionString { get; set; }
        public int PauseInStartupMs { get; set; }
        public RfidOptions InitialRfidOptions { get; set; }
    }
}