using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.PlatformServices;
using System.Threading;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSessionService
    {
        private readonly IAutoMapperProvider autoMapperProvider;
        private readonly ISystemClock clock;
        private readonly IEventRepository eventRepository;
        private readonly IRecordingService recordingService;
        private readonly IMessageHub messageHub;
        private readonly SemaphoreSlim sync = new(1);

        public TimingSessionService(IEventRepository eventRepository, IRecordingService recordingService, IMessageHub messageHub, 
            IAutoMapperProvider autoMapperProvider, ISystemClock clock)
        {
            this.eventRepository = eventRepository;
            this.recordingService = recordingService;
            this.messageHub = messageHub;
            this.autoMapperProvider = autoMapperProvider;
            this.clock = clock;
        }

        /// <summary>
        /// Публичный TimingSession не нужен, сервис всё делает, обновляет DTO сразу в хранилище
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TimingSession GetTimingSession(Id<TimingSessionDto> id)
        {
            var dto = eventRepository.GetRawDtoById(id);
            if (dto == null)
                return null;
            return new TimingSession(id, eventRepository, recordingService, messageHub, clock);
        }

        public TimingSession CreateSession(string name, Id<EventDto> eventId, Id<SessionDto> sessionId,
            Id<RecordingSessionDto> recordingSessionSessionId)
        {
            var dto = new TimingSessionDto
            {
                Name = name,
                EventId = eventId,
                SessionId = sessionId,
                RecordingSessionId = recordingSessionSessionId
            };
            eventRepository.Save(dto);
            return new TimingSession(dto.Id, eventRepository, recordingService, messageHub, clock);
        }
    }

    public class TimingSessionRatingUpdated
    {
        public List<RoundPosition> Rating { get; set; }
    }

    public class TimingSessionStaticData
    {
        public SessionDto SessionDefinition { get; set; }
        public IFinishCriteria FinishCriteria { get; set; }
        public TimeSpan MinLap { get; set; }
        public ConcurrentDictionary<string, List<Id<RiderClassRegistrationDto>>> RiderIdMap { get; set; }
    }
}