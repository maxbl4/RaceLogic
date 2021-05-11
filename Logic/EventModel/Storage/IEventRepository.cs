using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public interface IEventRepository: IRepository
    {
        IEnumerable<RiderClassRegistrationDto> GetRegistrations(Id<ClassDto> classId, Id<EventDto> eventId);
        List<RiderClassRegistrationDto> GetRegistrations(Id<SessionDto> sessionId);
        Dictionary<string, List<Id<RiderClassRegistrationDto>>> GetRiderIdentifiers(Id<SessionDto> sessionId);
        T GetWithUpstream<T>(Id<T> id) where T : IHasId<T>;
        IEnumerable<SessionDto> ListSessions(Id<EventDto> id);
        IEnumerable<TimingSessionDto> ListTimingSessions(Id<SessionDto> id);
    }
}