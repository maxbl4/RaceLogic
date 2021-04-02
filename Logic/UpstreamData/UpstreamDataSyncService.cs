using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Reactive.PlatformServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BraaapWeb.Client;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.Logic.UpstreamData
{
    public class UpstreamDataSyncService
    {
        private readonly UpstreamDataSyncServiceOptions options;
        private readonly IMainClient mainClient;
        private readonly UpstreamDataStorageService storageService;
        private readonly SemaphoreSlim sync = new(1);

        public UpstreamDataSyncService(IOptions<UpstreamDataSyncServiceOptions> options, IMainClient mainClient, UpstreamDataStorageService storageService)
        {
            this.options = options.Value;
            this.mainClient = mainClient;
            this.storageService = storageService;
        }
        
        public async Task<bool> Download(bool forceFullSync = false)
        {
            if (!await sync.WaitAsync(0))
                return false;
            try
            {
                if (forceFullSync)
                    storageService.PurgeExistingData();
                var lastSyncTimestamp = forceFullSync ? Constants.DefaultUtcDate : storageService.GetLastSyncTimestamp();
                var series = await mainClient.SeriesAsync(options.ApiKey, lastSyncTimestamp);
                var championships = await mainClient.ChampionshipsAsync(options.ApiKey, lastSyncTimestamp);
                var classes = await mainClient.ClassesAsync(options.ApiKey, lastSyncTimestamp);
                var events = await mainClient.EventsAsync(options.ApiKey, lastSyncTimestamp);
                var eventPrices = await mainClient.EventPricesAsync(options.ApiKey, lastSyncTimestamp);
                var schedules = await mainClient.SchedulesAsync(options.ApiKey, lastSyncTimestamp);
                var scheduleToClass = await mainClient.ScheduleToClassAsync(options.ApiKey, lastSyncTimestamp);
                var eventConfirmations = await mainClient.EventConfirmationsAsync(options.ApiKey, lastSyncTimestamp);
                var riderProfiles = await mainClient.RiderProfilesAsync(options.ApiKey, lastSyncTimestamp);
                var riderRegistrations = await mainClient.RiderRegistrationsAsync(options.ApiKey, lastSyncTimestamp);
                storageService.UpsertSeries(series.ToDto());
                storageService.UpsertChampionships(championships.ToDto());
                storageService.UpsertClasses(classes.ToDto());
                storageService.UpsertEvents(events.ToDto(eventPrices));
                storageService.UpsertSessions(schedules.ToDto(scheduleToClass));
                

                storageService.UpsertRiderProfiles(riderProfiles.ToDto());
                return true;
            }
            finally
            {
                sync.Release();
            }
        }
    }

    public class UpstreamDataSyncServiceOptions
    {
        public string BaseUri { get; set; }
        public string ApiKey { get; set; }
        public string StorageConnectionString { get; set; }
    }
}