using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.PlatformServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BraaapWeb.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.UpstreamData
{
    public class UpstreamDataSyncService
    {
        private readonly UpstreamDataSyncServiceOptions options;
        private readonly IMainClient mainClient;
        private readonly UpstreamDataStorageService storageService;
        private readonly ISystemClock systemClock;
        private readonly SemaphoreSlim sync = new(1);

        public UpstreamDataSyncService(UpstreamDataSyncServiceOptions options, IMainClient mainClient, UpstreamDataStorageService storageService, ISystemClock systemClock)
        {
            this.options = options;
            this.mainClient = mainClient;
            this.storageService = storageService;
            this.systemClock = systemClock;
        }
        
        public async Task<bool> Download(bool forceFullSync = false)
        {
            if (!await sync.WaitAsync(0))
                return false;
            try
            {
                var lastSyncTimestamp = forceFullSync ? Constants.DefaultUtcDate : storageService.GetLastSyncTimestamp();
                storageService.UpsertSeries((await mainClient.SeriesAsync(options.ApiKey, lastSyncTimestamp)).ToDto());
                storageService.UpsertChampionships((await mainClient.ChampionshipsAsync(options.ApiKey, lastSyncTimestamp)).ToDto());
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
        
        public static IEnumerable<ChampionshipDto> ToDto(this IEnumerable<Championship> entities)
        {
            return entities.Select(ToDto);
        }
        
        public static SeriesDto ToDto(this Series entity)
        {
            return new SeriesDto
            {
                Id = entity.SeriesId,
                Name = entity.Name,
                Description = entity.Description,
                Published = entity.Published,
                IsSeed = entity.Seed,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
        
        public static ChampionshipDto ToDto(this Championship entity)
        {
            return new ChampionshipDto
            {
                Id = entity.ChampionshipId,
                SeriesId = entity.SeriesId,
                Name = entity.Name,
                Description = entity.Description,
                Published = entity.Published,
                IsSeed = entity.Seed,
                Created = entity.Created.UtcDateTime,
                Updated = entity.Updated.UtcDateTime,
            };
        }
    }

    public class UpstreamDataSyncServiceOptions
    {
        public string BaseUri { get; set; }
        public string ApiKey { get; set; }
    }
}