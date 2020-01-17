using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class SessionService
    {
        private readonly IEventRepository eventRepository;

        public SessionService(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public RecordingSession StartSession(Id<SessionDto> sessionDtoId, string name)
        {
            var recordingSession = new RecordingSession {Name = name, SessionDtoId = sessionDtoId};
            eventRepository.Save(recordingSession);
            return recordingSession;
        }

        public void RecordCheckpoint(RecordingSession recordingSession, Checkpoint checkpoint)
        {
            
        }
    }
}