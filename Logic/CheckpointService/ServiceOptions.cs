using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.WsHub.Subscriptions;

namespace maxbl4.Race.Logic.CheckpointService
{
    public class ServiceOptions
    {
        public string StorageConnectionString { get; set; }
        public int PauseInStartupMs { get; set; }
        public RfidOptions InitialRfidOptions { get; set; }
        public UpstreamOptions InitialUpstreamOptions { get; set; }
    }
}