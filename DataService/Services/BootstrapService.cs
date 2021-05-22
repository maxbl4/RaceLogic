using System.Threading;
using System.Threading.Tasks;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventStorage.Storage;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Race.DataService.Services
{
    public class BootstrapService: IHostedService
    {
        private readonly ISeedDataLoader seedDataLoader;
        private readonly ITimingSessionService timingSessionService;
        private readonly IRecordingService recordingService;

        public BootstrapService(ISeedDataLoader seedDataLoader, ITimingSessionService timingSessionService, IRecordingService recordingService)
        {
            this.seedDataLoader = seedDataLoader;
            this.timingSessionService = timingSessionService;
            this.recordingService = recordingService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            seedDataLoader.Load(false);
            recordingService.Initialize();
            timingSessionService.Initialize();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}