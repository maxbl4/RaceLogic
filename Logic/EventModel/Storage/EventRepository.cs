using System.Collections.Generic;
using System.Linq;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
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

        public Id<GateDto> GetGateId(Id<SessionDto> sessionId)
        {
            var session = GetWithUpstream(sessionId);
            var ev = GetWithUpstream(session.EventId);
            return StorageService
                .List<GateDto>(x => x.OrganizationId == ev.OrganizationId)
                .FirstOrDefault()
                ?.Id ?? Id<GateDto>.Empty;
        }

        public T GetWithUpstream<T>(Id<T> id) where T: IHasId<T>
        {
            var ev = StorageService.Get(id) ?? upstreamDataRepository.Get(id);
            return ev;
        }
        
        public IEnumerable<SessionDto> ListSessions(Id<EventDto> id)
        {
            return StorageService.List<SessionDto>(x => x.EventId == id)
                .Concat(upstreamDataRepository.ListSessions(id))
                .OrderBy(x => x.StartTime);
        }
        
        public IEnumerable<TimingSessionDto> ListTimingSessions(Id<SessionDto> id)
        {
            return StorageService.List<TimingSessionDto>(x => x.SessionId == id)
                .OrderBy(x => x.StartTime);
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
            var session = GetWithUpstream(sessionId);
            var t = upstreamDataRepository.ListEventRegistrations(session.ClassIds)
                .SelectMany(x => x.Identifiers, (dto, id) => new {RiderId = dto.RiderClassRegistrationId, Identifier = id})
                .GroupBy(x => x.Identifier, x => x.RiderId)
                .ToDictionary(x => x.Key, x => x.ToList());
            return t;
        }

        public IStorageService StorageService { get; }
    }
}