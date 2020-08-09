using System.Net;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;

namespace maxbl4.Race.Tests.CheckpointService.RfidSimulator
{
    public class SimulatorBuilder
    {
        private readonly StorageService storageService;

        public SimulatorBuilder(StorageService storageService)
        {
            this.storageService = storageService;
        }
        
        public TagListHandler Build(bool enableRfid = true)
        {
            var readerEndpoint = IPEndPoint.Parse("127.0.0.1:0");
            var simulator = new SimulatorListener(readerEndpoint);
            var tagListHandler = new TagListHandler();
            simulator.TagListHandler = tagListHandler.Handle;
            var serializedConnectionString = $"Protocol = Alien; Network = {simulator.ListenEndpoint}";
            var settings = storageService.GetRfidOptions();
            settings.Enabled = enableRfid;
            settings.ConnectionString = serializedConnectionString;
            storageService.SetRfidOptions(settings);
            return tagListHandler;
        }
    }
}