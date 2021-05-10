using System.Collections.Generic;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class EventRepository : IEventRepository
    {
        private readonly IUpstreamDataRepository upstreamDataRepository;

        public EventRepository(IStorageService storageService, IUpstreamDataRepository upstreamDataRepository)
        {
            this.upstreamDataRepository = upstreamDataRepository;
            StorageService = storageService;
        }

        public EventDto GetEvent(Id<EventDto> id)
        {
            var ev = StorageService.Get(id);
            if (ev == null)
                ev = upstreamDataRepository.Get(id);
            return ev;
        }

        public List<SessionDto> ListSessions(Id<EventDto> id)
        {
            var sessions = StorageService.List<SessionDto>(x => x.EventId == id);
            sessions.AddRange(upstreamDataRepository.ListSessions(id));
            return sessions;
        }
        
        public IEnumerable<RiderClassRegistrationDto> GetRegistrations(Id<ClassDto> classId, Id<EventDto> eventId)
        {
            yield break;
        }

        public List<RiderClassRegistrationDto> GetRegistrations(Id<SessionDto> sessionId)
        {
            var session = StorageService.Get(sessionId);
            return null;
        }

        public Dictionary<string, List<Id<RiderClassRegistrationDto>>> GetRiderIdentifiers(Id<SessionDto> sessionId)
        {
            return null;
        }

        public IStorageService StorageService { get; }
    }
}