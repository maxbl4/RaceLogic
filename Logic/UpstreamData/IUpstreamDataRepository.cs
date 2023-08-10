using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.UpstreamData
{
    public interface IUpstreamDataRepository: IRepository
    {
        DateTime GetLastSyncTimestamp();
        void PurgeExistingData();
        void UpsertSeries(IEnumerable<SeriesDto> entities);
        void UpsertChampionships(IEnumerable<ChampionshipDto> entities);
        void UpsertClasses(IEnumerable<ClassDto> entities);
        void UpsertEvents(IEnumerable<EventDto> entities);
        void UpsertSessions(IEnumerable<SessionDto> entities);
        void UpsertRiderRegistrations(IEnumerable<RiderClassRegistrationDto> entities);
        void UpsertEventRegistrations(IEnumerable<RiderEventRegistrationDto> entities);
        IEnumerable<SeriesDto> ListSeries();
        IEnumerable<ChampionshipDto> ListChampionships(Id<SeriesDto>? seriesId = null);
        IEnumerable<ClassDto> ListClasses(Id<ChampionshipDto>? championshipId = null);
        IEnumerable<EventDto> ListEvents(Id<ChampionshipDto>? championshipId = null);
        IEnumerable<SessionDto> ListSessions(Id<EventDto>? eventId = null);
        T Get<T>(Id<T> id) where T : IHasId<T>;
        IEnumerable<RiderEventRegistrationDto> ListEventRegistrations(IEnumerable<Id<ClassDto>> classIds);
        IEnumerable<RiderClassRegistrationDto> ListClassRegistrations(IEnumerable<Id<ClassDto>> classIds);
        IEnumerable<ClassDto> ListClasses(IEnumerable<Id<ClassDto>> classIds);
        IEnumerable<ClassDto> ListClasses(Id<SessionDto> sessionId);
        IEnumerable<RiderClassRegistrationDto> ListClassRegistrations(Id<ChampionshipDto> championshipId);
    }
}