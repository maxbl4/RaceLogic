using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.PlatformServices;
using System.Threading;
using System.Threading.Tasks;
using BraaapWeb.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.PeerData
{
    public class PeerDataSyncService
    {
        private readonly PeerDataSyncServiceOptions options;
        private readonly IMainClient mainClient;
        private readonly PeerDataStorageService storageService;
        private readonly ISystemClock systemClock;
        private readonly SemaphoreSlim sync = new(1);

        public PeerDataSyncService(PeerDataSyncServiceOptions options, IMainClient mainClient, PeerDataStorageService storageService, ISystemClock systemClock)
        {
            this.options = options;
            this.mainClient = mainClient;
            this.storageService = storageService;
            this.systemClock = systemClock;
        }
        
        public async Task<bool> Download(Id<PeerDatabaseDto> peerId,bool forceFullSync = false)
        {
            if (!await sync.WaitAsync(0))
                return false;
            try
            {
                var peer = storageService.GetPeerDatabase(peerId);
                var lastSyncTimestamp = forceFullSync ? Constants.DefaultUtcDate : peer.LastSyncTimestamp;
                var client = new MainClient(peer.BaseUri, new HttpClient());
                storageService.ReplaceSeries((await client.SeriesAsync(peer.ApiKey, lastSyncTimestamp)).ToDto(),
                    forceFullSync);
                // storageService.ReplaceChampionships(await client.ChampionshipsAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceClasses(await client.ClassesAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceEvents(await client.EventsAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceEventConfirmations(await client.EventConfirmationsAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceSchedules(await client.SchedulesAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceScheduleToClasses(await client.ScheduleToClassAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceRiderProfiles(await client.RiderProfilesAsync(peer.ApiKey, loadFrom), forceFullSync);
                // storageService.ReplaceRiderRegistrations(await client.RiderRegistrationsAsync(peer.ApiKey, loadFrom), forceFullSync);
                return true;
            }
            finally
            {
                sync.Release();
            }
        }
    }

    public static class DtoMapper
    {
        public static IEnumerable<SeriesDto> ToDto(this IEnumerable<Series> entities)
        {
            return entities.Select(ToDto);
        }
        
        public static SeriesDto ToDto(this Series entity)
        {
            return new SeriesDto();
        }
        
        public static Series ToBraaap(this SeriesDto entity)
        {
            return new Series();
        }
    }

    public class PeerDataSyncServiceOptions
    {
    }
}