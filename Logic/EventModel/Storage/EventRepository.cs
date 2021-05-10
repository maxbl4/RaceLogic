using System.Collections.Generic;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class EventRepository : IEventRepository
    {
        public EventRepository(IStorageService storageService)
        {
            StorageService = storageService;
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