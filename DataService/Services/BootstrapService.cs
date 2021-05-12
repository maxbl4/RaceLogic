using System.Threading;
using System.Threading.Tasks;
using maxbl4.Race.Logic.EventModel.Runtime;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Race.DataService.Services
{
    public class BootstrapService: IHostedService
    {
        public BootstrapService(ITimingSessionService timingSessionService, IRecordingService recordingService)
        {
            
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}