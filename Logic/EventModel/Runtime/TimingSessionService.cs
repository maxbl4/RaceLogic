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
            var session = eventRepository.GetRawDtoById<SessionDto>(sessionDtoId);
            var recordingSessionDto = new RecordingSessionDto {Name = name, SessionDtoId = sessionDtoId};
            eventRepository.Save(recordingSessionDto);
            var timingSession = autoMapperProvider.Map<TimingSession>(recordingSessionDto);
            timingSession.MinLap = session.MinLap;
            timingSession.FinishCriteria = new FinishCriteria(eventRepository.GetRawDtoById(session.FinishCriteriaId));
            return timingSession;
        }

        public void RecordCheckpoint(TimingSession timingSession, Checkpoint checkpoint)
        {
            var checkpointDto = autoMapperProvider.Map<CheckpointDto>(checkpoint);
            eventRepository.Save(checkpointDto);
            checkpointDto.RecordingSessionId = timingSession.RecordingSessionId;
            timingSession.Checkpoints.Add(checkpoint);
        }
    }
}