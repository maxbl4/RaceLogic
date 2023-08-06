using System.Collections.Generic;
using System.Reactive.PlatformServices;
using System.Threading;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.EventModel.Storage;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSessionService : ITimingSessionService
    {
        private readonly IAutoMapperProvider autoMapperProvider;
        private readonly ISystemClock clock;
        private readonly IEventRepository eventRepository;
        private readonly IMessageHub messageHub;
        private readonly SemaphoreSlim sync = new(1);
        public TimingSession ActiveSession { get; private set; }

        public TimingSessionService(IEventRepository eventRepository, IMessageHub messageHub, 
            IAutoMapperProvider autoMapperProvider, ISystemClock clock)
        {
            this.eventRepository = eventRepository;
            this.messageHub = messageHub;
            this.autoMapperProvider = autoMapperProvider;
            this.clock = clock;
        }

        public void Initialize()
        {
            
        }
        
        public void StopSession()
        {
            using var _ = sync.UseOnce();
            if (ActiveSession == null) return;
            eventRepository.StorageService.Update(ActiveSession.Id, x =>
            {
                x.Stop(clock.UtcNow.UtcDateTime);
            });
            ActiveSession.DisposeSafe();
            ActiveSession = null;
        }

        public void StartNewSession(string name, Id<SessionDto> sessionId)
        {
            using var _ = sync.UseOnce();
            var session = eventRepository.GetWithUpstream(sessionId);
            var dto = new TimingSessionDto
            {
                Name = name,
                EventId = session.EventId,
                SessionId = sessionId,
                StartTime = clock.UtcNow.UtcDateTime
            };
            eventRepository.StorageService.Save(dto);
            ActiveSession = new TimingSession(dto.Id, eventRepository, 
                autoMapperProvider, messageHub, clock);
        }
    }
}