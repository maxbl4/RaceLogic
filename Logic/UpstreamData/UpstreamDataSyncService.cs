using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BraaapWeb.Client;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using Microsoft.Extensions.Options;

namespace maxbl4.Race.Logic.UpstreamData
{
    public class UpstreamDataSyncService
    {
        private readonly UpstreamDataSyncServiceOptions options;
        private readonly IMainClient mainClient;
        private readonly IUpstreamDataRepository repository;
        private readonly IMessageHub messageHub;
        private readonly SemaphoreSlim sync = new(1);

        public UpstreamDataSyncService(IOptions<UpstreamDataSyncServiceOptions> options, IMainClient mainClient, IUpstreamDataRepository repository,
            IMessageHub messageHub)
        {
            this.options = options.Value;
            this.mainClient = mainClient;
            this.repository = repository;
            this.messageHub = messageHub;
        }
        
        public async Task<bool> Download(bool forceFullSync = false)
        {
            if (!await sync.WaitAsync(0))
                return false;
            try
            {
                if (forceFullSync)
                    repository.PurgeExistingData();
                var lastSyncTimestamp = forceFullSync ? Constants.DefaultUtcDate : repository.GetLastSyncTimestamp();
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
                var disqualifications = await mainClient.RiderDisqualificationsAsync(options.ApiKey);
                repository.UpsertSeries(series.ToDto());
                repository.UpsertChampionships(championships.ToDto());
                repository.UpsertClasses(classes.ToDto());
                repository.UpsertEvents(events.ToDto(eventPrices));
                repository.UpsertSessions(schedules.ToDto(scheduleToClass));
                
                var riderRegs = from rr in riderRegistrations
                    join cl in classes on rr.ClassId equals  cl.ClassId
                    join rp in riderProfiles on rr.RiderProfileId equals rp.Id
                    join dsq in disqualifications on new {rr.RiderRegistrationId, rr.ClassId} equals new {dsq.RiderRegistrationId, ClassId = dsq.ClassId??Guid.Empty} 
                        into hasDsq
                    let rider = new {Profile = rp, Reg = rr, Class = cl, HasDsq = hasDsq.Any()}
                    select rider;
                repository.UpsertRiderRegistrations(riderRegs.Select(riderReg => new RiderClassRegistrationDto
                {
                    Id = riderReg.Reg.RiderRegistrationId,
                    RiderProfileId = riderReg.Reg.RiderProfileId,
                    ClassId = riderReg.Reg.ClassId,
                    ChampionshipDtoId = riderReg.Class.ChampionshipId,
                    Moto = riderReg.Reg.Moto,
                    Number = riderReg.Reg.Number,
                    Validated = riderReg.Reg.Validated,
                    ValidatedDate = riderReg.Reg.ValidatedDate?.UtcDateTime ?? Constants.DefaultUtcDate,
                    IsDisqualified = riderReg.HasDsq,
                    IsSeed = riderReg.Profile.Seed,
                    Created = riderReg.Reg.Created.UtcDateTime,
                    Updated = riderReg.Reg.Updated.UtcDateTime,
                    Birthdate = riderReg.Profile.Birthdate.UtcDateTime,
                    RiderDescription = riderReg.Profile.City,
                    FirstName = riderReg.Profile.FirstName,
                    ParentName = riderReg.Profile.ParentName,
                    LastName = riderReg.Profile.LastName,
                    IdentityConfirmed = riderReg.Profile.IsActive,
                    IdentityConfirmedDate = riderReg.Reg.Updated.UtcDateTime,
                }));
                

                var eventRegs = from rr in riderRegistrations
                    join ec in eventConfirmations on rr.RiderRegistrationId equals ec.RiderRegistrationId
                    join dsq in disqualifications on new {rr.RiderRegistrationId, ec.EventId} equals new {dsq.RiderRegistrationId, EventId = dsq.EventId??Guid.Empty} 
                        into hasDsq
                    select new {Ec = ec, Rr = rr, HasDsq = hasDsq.Any()};
                repository.UpsertEventRegistrations(eventRegs.Select(x => new RiderEventRegistrationDto
                {
                    Id = x.Ec.EventConfirmationId,
                    EventId = x.Ec.EventId,
                    ClassId = x.Rr.ClassId,
                    RiderClassRegistrationId = x.Rr.RiderRegistrationId, 
                    Identifiers = new HashSet<string>(new []{x.Rr.Number.ToString(), x.Rr.TagSimple}.Where(t => t != null)),
                    Paid = x.Ec.Paid,
                    PaymentConfirmed = x.Ec.PaymentConfirmed,
                    OrdinalIndex = x.Ec.OrdinalIndex,
                    IsDisqualified = x.HasDsq,
                    Validated = x.Ec.Validated,
                    ValidatedDate = x.Ec.ValidatedDate?.UtcDateTime ?? Constants.DefaultUtcDate,
                    Created = x.Ec.Created.UtcDateTime,
                    Updated = x.Ec.Updated.UtcDateTime,
                }));
                messageHub.Publish(new UpstreamDataSyncComplete());
                return true;
            }
            finally
            {
                sync.Release();
            }
        }
    }

    public class UpstreamDataSyncComplete
    {
    }

    public class UpstreamDataSyncServiceOptions
    {
        public string BaseUri { get; set; }
        public string ApiKey { get; set; }
    }
}