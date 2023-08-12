using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.DataService.Hubs;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.WebModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Race.DataService.Services
{
    public class BootstrapService: IHostedService
    {
        private readonly ISeedDataLoader seedDataLoader;
        private readonly ITimingSessionService timingSessionService;
        private readonly IHubContext<RaceHub> raceHub;
        private readonly IMessageHub messageHub;

        public BootstrapService(ISeedDataLoader seedDataLoader, 
            ITimingSessionService timingSessionService, 
            IHubContext<RaceHub> raceHub, 
            IMessageHub messageHub)
        {
            this.seedDataLoader = seedDataLoader;
            this.timingSessionService = timingSessionService;
            this.raceHub = raceHub;
            this.messageHub = messageHub;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            SetupWsProxy();
            seedDataLoader.Load(false);
            timingSessionService.Initialize();
            return Task.CompletedTask;
        }

        private void SetupWsProxy()
        {
            CreateWsProxy<TimingSessionUpdate>();
            CreateWsProxy<ActiveTimingSessionsUpdate>();
        }

        private void CreateWsProxy<T>()
        {
            messageHub.SubscribeAsync<T>(async x => 
                await raceHub.Clients.All.SendAsync(typeof(T).Name, x));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}