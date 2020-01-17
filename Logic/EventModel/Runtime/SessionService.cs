using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class SessionService
    {
        private readonly IEventRepository eventRepository;
        private readonly IAutoMapperProvider autoMapperProvider;

        public SessionService(IEventRepository eventRepository, IAutoMapperProvider autoMapperProvider)
        {
            this.eventRepository = eventRepository;
            this.autoMapperProvider = autoMapperProvider;
        }

        public RecordingSession StartSession(Id<SessionDto> sessionDtoId, string name)
        {
            var recordingSession = new RecordingSession {Name = name, SessionDtoId = sessionDtoId};
            eventRepository.Save(recordingSession);
            return recordingSession;
        }

        public void RecordCheckpoint(RecordingSession recordingSession, Checkpoint checkpoint)
        {
            var checkpointDto = autoMapperProvider.Mapper.Map<CheckpointDto>(checkpoint);
        }
    }
}