using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEventRepository eventRepository;
        private readonly IAutoMapperProvider autoMapperProvider;

        public TimingSessionService(IEventRepository eventRepository, IAutoMapperProvider autoMapperProvider)
        {
            this.eventRepository = eventRepository;
            this.autoMapperProvider = autoMapperProvider;
        }

        public TimingSession StartSession(Id<SessionDto> sessionDtoId, string name)
        {
            var staticData = LoadTimingSessionStaticData(sessionDtoId);
            var recordingSessionDto = new RecordingSessionDto {Name = name, SessionDtoId = sessionDtoId};
            eventRepository.Save(recordingSessionDto);
            var timingSession = autoMapperProvider.Map<TimingSession>(recordingSessionDto);
            timingSession.MinLap = staticData.MinLap;
            timingSession.FinishCriteria = staticData.FinishCriteria;
            return timingSession;
        }

        public TimingSession ResumeSession(Id<RecordingSessionDto> recordingSessionId)
        {
            return default;
        }

        public void RecordCheckpoint(TimingSession timingSession, Checkpoint checkpoint)
        {
            var checkpointDto = autoMapperProvider.Map<CheckpointDto>(checkpoint);
            eventRepository.Save(checkpointDto);
            checkpointDto.RecordingSessionId = timingSession.RecordingSessionId;
            timingSession.AppendCheckpoint(checkpoint);
        }

        private TimingSessionStaticData LoadTimingSessionStaticData(Id<SessionDto> sessionId)
        {
            var session = eventRepository.GetRawDtoById(sessionId);
            return new TimingSessionStaticData
            {
                SessionDefinition = session,
                MinLap = session.MinLap,
                FinishCriteria = new FinishCriteria(eventRepository.GetRawDtoById(session.FinishCriteriaId)),
                RiderIdMap = LoadRiderIdMap(sessionId)
            };
        }

        private ConcurrentDictionary<string, List<Id<RiderProfileDto>>> LoadRiderIdMap(Id<SessionDto> sessionId)
        {
            var riderIdentifiers = eventRepository.GetRiderIdentifiers(sessionId);
            var riderIdMap = new ConcurrentDictionary<string, List<Id<RiderProfileDto>>>();
            foreach (var idGroup in riderIdentifiers.GroupBy(x => x.Identifier))
            {
                riderIdMap[idGroup.Key] = idGroup.Select(x => x.RiderProfileId).ToList();
            }
            return riderIdMap;
        }
    }

    public class TimingSessionStaticData
    {
        public SessionDto SessionDefinition { get; set; }
        public IFinishCriteria FinishCriteria { get; set; }
        public TimeSpan MinLap { get; set; }
        public ConcurrentDictionary<string, List<Id<RiderProfileDto>>> RiderIdMap { get; set; }
    }
}