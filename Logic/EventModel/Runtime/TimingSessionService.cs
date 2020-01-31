using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
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
        private readonly SemaphoreSlim sync = new SemaphoreSlim(1);
        private readonly IEventRepository eventRepository;
        private readonly IAutoMapperProvider autoMapperProvider;
        public RecordingSessionDto ActiveRecordingSession { get; private set; }
        public TimingSession ActiveTimingSession { get; private set; }

        public TimingSessionService(IEventRepository eventRepository, IAutoMapperProvider autoMapperProvider)
        {
            this.eventRepository = eventRepository;
            this.autoMapperProvider = autoMapperProvider;
        }

        public TimingSession StartSession(Id<SessionDto> sessionDtoId, string name)
        {
            var recordingSessionDto = ActiveRecordingSession = new RecordingSessionDto {Name = name, SessionId = sessionDtoId};
            eventRepository.Save(recordingSessionDto);
            return ActiveTimingSession = CreateTimingSession(recordingSessionDto);
        }

        public TimingSession ResumeSession(Id<RecordingSessionDto> recordingSessionId)
        {
            using var s = sync.UseOnce();
            var recordingSessionDto = ActiveRecordingSession = eventRepository.GetRawDtoById(recordingSessionId);
            return ActiveTimingSession = CreateTimingSession(recordingSessionDto);
        }

        public void RecordCheckpoint(Checkpoint checkpoint)
        {
            using var s = sync.UseOnce();
            var checkpointDto = autoMapperProvider.Map<CheckpointDto>(checkpoint);
            checkpointDto.RecordingSessionId = ActiveRecordingSession.Id;
            eventRepository.Save(checkpointDto);
            ActiveTimingSession.AppendCheckpoint(checkpoint);
        }

        private TimingSession CreateTimingSession(RecordingSessionDto recordingSessionDto)
        {
            var staticData = LoadTimingSessionStaticData(recordingSessionDto.SessionId);
            var timingSession = ActiveTimingSession = autoMapperProvider.Map<TimingSession>(recordingSessionDto);
            timingSession.StartTime = recordingSessionDto.Created;
            timingSession.MinLap = staticData.MinLap;
            timingSession.FinishCriteria = staticData.FinishCriteria;
            timingSession.Initialize(autoMapperProvider.Map<List<Checkpoint>>(eventRepository.GetRawDtos<CheckpointDto>(x => x.RecordingSessionId == recordingSessionDto.Id)));
            return timingSession;
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