using System.Collections.Generic;
using System.Reactive.PlatformServices;
using System.Threading;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
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
        private readonly IRecordingService recordingService;
        private readonly IRecordingServiceRepository recordingRepository;
        private readonly IMessageHub messageHub;
        private readonly SemaphoreSlim sync = new(1);
        private readonly Dictionary<Id<TimingSessionDto>, TimingSession> activeSessions = new();

        public TimingSessionService(IEventRepository eventRepository, IRecordingService recordingService, IRecordingServiceRepository recordingRepository, IMessageHub messageHub, 
            IAutoMapperProvider autoMapperProvider, ISystemClock clock)
        {
            this.eventRepository = eventRepository;
            this.recordingService = recordingService;
            this.recordingRepository = recordingRepository;
            this.messageHub = messageHub;
            this.autoMapperProvider = autoMapperProvider;
            this.clock = clock;
        }

        public void Initialize()
        {
            
        }

        public void StartSession(Id<TimingSessionDto> id)
        {
            eventRepository.StorageService.Update(id, x =>
            {
                x.Start(clock.UtcNow.UtcDateTime);
            });
        }
        
        public void StopSession(Id<TimingSessionDto> id)
        {
            eventRepository.StorageService.Update(id, x =>
            {
                x.Stop(clock.UtcNow.UtcDateTime);
            });
        }

        public TimingSession CreateSession(string name, Id<SessionDto> sessionId)
        {
            var session = eventRepository.GetWithUpstream(sessionId);
            var dto = new TimingSessionDto
            {
                Name = name,
                EventId = session.EventId,
                SessionId = sessionId,
            };
            eventRepository.StorageService.Save(dto);
            return new TimingSession(dto.Id, eventRepository, recordingService, 
                recordingRepository, autoMapperProvider, messageHub, clock);
        }
    }
}