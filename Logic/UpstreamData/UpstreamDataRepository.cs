using System;
using System.Collections.Generic;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
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
        string Name<T>();
    }

    public class UpstreamDataRepository: IUpstreamDataRepository
    {
        public UpstreamDataRepository(IStorageService storageService)
        {
            StorageService = storageService;
        }

        public IStorageService StorageService { get; }

        void IRepository.ValidateDatabase(ILiteRepository repo)
        {
        }

        void IRepository.SetupIndexes(ILiteRepository repo)
        {
            
        }

        public DateTime GetLastSyncTimestamp()
        {
            return StorageService.Repo.Query<Timestamp>(Name<Timestamp>()).FirstOrDefault()?.LastSyncTimeStamp ?? Constants.DefaultUtcDate;
        }

        public void PurgeExistingData()
        {
            StorageService.Repo.DeleteMany<SeriesDto>(x => true, Name<SeriesDto>());
            StorageService.Repo.DeleteMany<ChampionshipDto>(x => true, Name<ChampionshipDto>());
            StorageService.Repo.DeleteMany<ClassDto>(x => true, Name<ClassDto>());
            StorageService.Repo.DeleteMany<EventDto>(x => true, Name<EventDto>());
            StorageService.Repo.DeleteMany<SessionDto>(x => true, Name<SessionDto>());
            StorageService.Repo.DeleteMany<RiderClassRegistrationDto>(x => true, Name<RiderClassRegistrationDto>());
            StorageService.Repo.DeleteMany<RiderEventRegistrationDto>(x => true, Name<RiderEventRegistrationDto>());
        }

        public void UpsertSeries(IEnumerable<SeriesDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<SeriesDto>());
        }

        public void UpsertChampionships(IEnumerable<ChampionshipDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<ChampionshipDto>());
        }

        public void UpsertClasses(IEnumerable<ClassDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<ClassDto>());
        }

        public void UpsertEvents(IEnumerable<EventDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<EventDto>());
        }
        
        public void UpsertSessions(IEnumerable<SessionDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<SessionDto>());
        }

        public void UpsertRiderRegistrations(IEnumerable<RiderClassRegistrationDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<RiderClassRegistrationDto>());
        }
        
        public void UpsertEventRegistrations(IEnumerable<RiderEventRegistrationDto> entities)
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<RiderEventRegistrationDto>());
        }

        public IEnumerable<SeriesDto> ListSeries()
        {
            return StorageService.Repo.Query<SeriesDto>(Name<SessionDto>()).ToEnumerable();
        }
        
        public IEnumerable<ChampionshipDto> ListChampionships(Id<SeriesDto>? seriesId = null)
        {
            var query = StorageService.Repo.Query<ChampionshipDto>(Name<ChampionshipDto>());
            if (seriesId != null && seriesId != Id<SeriesDto>.Empty)
                query = query.Where(x => x.SeriesId == seriesId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<ClassDto> ListClasses(Id<ChampionshipDto>? championshipId = null)
        {
            var query = StorageService.Repo.Query<ClassDto>(Name<ClassDto>());
            if (championshipId != null && championshipId != Id<ChampionshipDto>.Empty)
                query = query.Where(x => x.ChampionshipId == championshipId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<EventDto> ListEvents(Id<ChampionshipDto>? championshipId = null)
        {
            var query = StorageService.Repo.Query<EventDto>(Name<EventDto>());
            if (championshipId != null && championshipId != Id<ChampionshipDto>.Empty)
                query = query.Where(x => x.ChampionshipId == championshipId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<SessionDto> ListSessions(Id<EventDto>? eventId = null)
        {
            var query = StorageService.Repo.Query<SessionDto>(Name<SessionDto>());
            if (eventId != null && eventId != Id<EventDto>.Empty)
                query = query.Where(x => x.EventId == eventId);
            return query.ToEnumerable();
        }

        public string Name<T>()
        {
            return typeof(T).Name + "Upstream";
        }

        private class Timestamp
        {
            public int Id { get; set; } = 1;
            public DateTime LastSyncTimeStamp { get; set; } = Constants.DefaultUtcDate;
        }
    }
}