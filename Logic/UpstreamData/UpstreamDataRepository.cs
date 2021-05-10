using System;
using System.Collections;
using System.Collections.Generic;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.UpstreamData
{
    public interface IUpstreamDataRepository
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
    }

    public class UpstreamDataRepository: IRepository, IUpstreamDataRepository
    {
        public UpstreamDataRepository(IStorageService storageService)
        {
            StorageService = storageService;
            SetupIndexes(storageService.Repo);
        }

        public IStorageService StorageService { get; }

        private void SetupIndexes(ILiteRepository repo)
        {
            repo.Database.GetCollection<ClassDto>(Name<ClassDto>()).EnsureIndex(x => x.ChampionshipId);
            repo.Database.GetCollection<EventDto>(Name<EventDto>()).EnsureIndex(x => x.ChampionshipId);
            repo.Database.GetCollection<EventDto>(Name<EventDto>()).EnsureIndex(x => x.EndOfRegistration);
            repo.Database.GetCollection<SessionDto>(Name<SessionDto>()).EnsureIndex(x => x.EventId);
            repo.Database.GetCollection<SessionDto>(Name<SessionDto>()).EnsureIndex(x => x.ClassIds);
            repo.Database.GetCollection<RiderClassRegistrationDto>(Name<RiderClassRegistrationDto>()).EnsureIndex(x => x.ClassId);
            repo.Database.GetCollection<RiderClassRegistrationDto>(Name<RiderClassRegistrationDto>()).EnsureIndex(x => x.RiderProfileId);
            repo.Database.GetCollection<RiderClassRegistrationDto>(Name<RiderClassRegistrationDto>()).EnsureIndex(x => x.ChampionshipDtoId);
            repo.Database.GetCollection<RiderEventRegistrationDto>(Name<RiderEventRegistrationDto>()).EnsureIndex(x => x.ClassId);
            repo.Database.GetCollection<RiderEventRegistrationDto>(Name<RiderEventRegistrationDto>()).EnsureIndex(x => x.EventId);
            repo.Database.GetCollection<RiderEventRegistrationDto>(Name<RiderEventRegistrationDto>()).EnsureIndex(x => x.RiderClassRegistrationId);
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
            Upsert(entities);
        }

        public void UpsertChampionships(IEnumerable<ChampionshipDto> entities)
        {
            Upsert(entities);
        }

        public void UpsertClasses(IEnumerable<ClassDto> entities)
        {
            Upsert(entities);
        }

        public void UpsertEvents(IEnumerable<EventDto> entities)
        {
            Upsert(entities);
        }
        
        public void UpsertSessions(IEnumerable<SessionDto> entities)
        {
            Upsert(entities);
        }

        public void UpsertRiderRegistrations(IEnumerable<RiderClassRegistrationDto> entities)
        {
            Upsert(entities);
        }
        
        public void UpsertEventRegistrations(IEnumerable<RiderEventRegistrationDto> entities)
        {
            Upsert(entities);
        }

        public IEnumerable<SeriesDto> ListSeries()
        {
            return Query<SeriesDto>().ToEnumerable();
        }
        
        public IEnumerable<ChampionshipDto> ListChampionships(Id<SeriesDto>? seriesId = null)
        {
            var query = Query<ChampionshipDto>();
            if (seriesId != null && seriesId != Id<SeriesDto>.Empty)
                query = query.Where(x => x.SeriesId == seriesId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<ClassDto> ListClasses(Id<ChampionshipDto>? championshipId = null)
        {
            var query = Query<ClassDto>();
            if (championshipId != null && championshipId != Id<ChampionshipDto>.Empty)
                query = query.Where(x => x.ChampionshipId == championshipId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<EventDto> ListEvents(Id<ChampionshipDto>? championshipId = null)
        {
            var query = Query<EventDto>();
            if (championshipId != null && championshipId != Id<ChampionshipDto>.Empty)
                query = query.Where(x => x.ChampionshipId == championshipId);
            return query.ToEnumerable();
        }
        
        public IEnumerable<SessionDto> ListSessions(Id<EventDto>? eventId = null)
        {
            var query = Query<SessionDto>();
            if (eventId != null && eventId != Id<EventDto>.Empty)
                query = query.Where(x => x.EventId == eventId);
            return query.ToEnumerable();
        }

        public T Get<T>(Id<T> id) where T : IHasId<T>
        {
            return Query<T>().Where(x => x.Id == id).FirstOrDefault();
        }
        
        private ILiteQueryable<T> Query<T>()
        {
            return StorageService.Repo.Query<T>(Name<T>());
        }
        
        private void Upsert<T>(IEnumerable<T> entities) where T: IHasId<T>
        {
            StorageService.Repo.Upsert(entities.ApplyTraits(skipTimestamp:true), Name<T>());
        }

        private string Name<T>()
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