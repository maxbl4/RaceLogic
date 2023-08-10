using System.Collections.Generic;
using System.Linq;
using LiteDB;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.AutoMapper;
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
        private readonly IAutoMapperProvider autoMapper;

        public EventRepository(IStorageService storageService, 
            IUpstreamDataRepository upstreamDataRepository,
            IAutoMapperProvider autoMapper)
        {
            this.upstreamDataRepository = upstreamDataRepository;
            this.autoMapper = autoMapper;
            StorageService = storageService;
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
        
        public IEnumerable<TimingSessionDto> ListStoredActiveTimingSessions()
        {
            return StorageService.List<TimingSessionDto>(x => x.IsRunning)
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

        public List<RiderEventInfoDto> ListRiderEventInfo(Id<TimingSessionDto> timingSessionId)
        {
            var timingSession = StorageService.Get(timingSessionId);
            var session = GetWithUpstream(timingSession.SessionId);
            var ev = GetWithUpstream(session.EventId);
            var classes = upstreamDataRepository.ListClasses(session.ClassIds)
                .ToDictionary(x => x.Id);
            var riders = upstreamDataRepository
                .ListClassRegistrations(session.ClassIds);
            return riders
                .Select(x => autoMapper
                    .Map<RiderEventInfoDto>(x)
                    .SetClassName(classes.Get(x.ClassId)?.Name))
                .ToList();
        }
        
        public List<RiderClassRegistrationDto> ListClassRegistrations(Id<ChampionshipDto> championshipId)
        {
            return upstreamDataRepository.ListClassRegistrations(championshipId).ToList();
        }

        public Dictionary<string, List<Id<RiderClassRegistrationDto>>> GetRiderIdentifiers(Id<SessionDto> sessionId)
        {
            var session = GetWithUpstream(sessionId);
            var t = upstreamDataRepository.ListEventRegistrations(session.ClassIds)
                .SelectMany(x => x.Identifiers, (dto, id) => new {RiderId = dto.RiderClassRegistrationId, Identifier = id})
                .Where(x => x.Identifier != null)
                .GroupBy(x => x.Identifier, x => x.RiderId)
                .ToDictionary(x => x.Key, x => x.ToList());
            return t;
        }

        public IStorageService StorageService { get; }
    }
}