using System.Reactive.PlatformServices;
using System.Threading;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

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

        public TimingSession CreateSession(string name, Id<SessionDto> sessionId)
        {
            var session = eventRepository.GetWithUpstream(sessionId);
            var recordingSessionSessionId = recordingRepository.GetSessionForEvent(session.EventId)?.Id ?? Id<RecordingSessionDto>.Empty;
            var dto = new TimingSessionDto
            {
                Name = name,
                EventId = session.EventId,
                SessionId = sessionId,
                RecordingSessionId = recordingSessionSessionId
            };
            eventRepository.StorageService.Save(dto);
            return new TimingSession(dto.Id, eventRepository, recordingService, messageHub, clock);
        }
    }
}