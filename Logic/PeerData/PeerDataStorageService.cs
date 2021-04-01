using System.Collections.Generic;
using System.Reactive.PlatformServices;
using BraaapWeb.Client;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.PeerData
{
    public class PeerDataStorageService: StorageServiceBase
    {
        public PeerDataStorageService(string connectionString, IMessageHub messageHub, ISystemClock systemClock) : base(connectionString)
        {
        }

        protected override void ValidateDatabase()
        {
        }

        protected override void SetupIndexes()
        {
        }

        public PeerDatabaseDto GetPeerDatabase(Id<PeerDatabaseDto> peerId)
        {
            return repo.Query<PeerDatabaseDto>().Where(x => x.Id == peerId).FirstOrDefault();
        }

        public void PurgeExistingData()
        {
            
        }

        public void ReplaceSeries(IEnumerable<SeriesDto> entities, bool purgeExisting)
        {
            if (purgeExisting)
                repo.DeleteMany<SeriesDto>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceChampionships(IEnumerable<ChampionshipDto> entities, bool purgeExisting)
        {
            repo.DeleteMany<ChampionshipDto>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceClasses(ICollection<ClassDto> entities, bool purgeExisting)
        {
            repo.DeleteMany<ClassDto>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceEvents(ICollection<EventDto> entities, bool purgeExisting)
        {
            repo.DeleteMany<EventDto>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceEventConfirmations(ICollection<EventConfirmation> entities, bool purgeExisting)
        {
            repo.DeleteMany<EventConfirmation>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceSchedules(ICollection<ScheduleItemDto> entities, bool purgeExisting)
        {
            repo.DeleteMany<ScheduleItemDto>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceScheduleToClasses(ICollection<ScheduleToClass> entities, bool purgeExisting)
        {
            repo.DeleteMany<ScheduleToClass>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceRiderProfiles(ICollection<RiderProfile> entities, bool purgeExisting)
        {
            repo.DeleteMany<RiderProfile>(x => true);
            repo.Insert(entities);
        }

        public void ReplaceRiderRegistrations(ICollection<RiderRegistration> entities, bool purgeExisting)
        {
            repo.DeleteMany<RiderRegistration>(x => true);
            repo.Insert(entities);
        }
    }
}