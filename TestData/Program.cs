using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using BraaapWeb.Client;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;

namespace TestData
{
    class Program
    {
        static async Task Main()
        {
            Randomizer.Seed = new Random(1);
            
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
            var options = config.GetSection(nameof(UpstreamDataSyncServiceOptions)).Get<UpstreamDataSyncServiceOptions>();
            Console.WriteLine("Downloading upstream data");
            var mainClient = new MainClient(options.BaseUri, new HttpClient());
            var series = await mainClient.SeriesAsync(options.ApiKey, null);
            var championships = await mainClient.ChampionshipsAsync(options.ApiKey, null);
            var classes = await mainClient.ClassesAsync(options.ApiKey, null);
            var events = await mainClient.EventsAsync(options.ApiKey, null);
            var eventPrices = (await mainClient.EventPricesAsync(options.ApiKey, null)).Select(Mask).ToList();
            var schedules = await mainClient.SchedulesAsync(options.ApiKey, null);
            var scheduleToClass = await mainClient.ScheduleToClassAsync(options.ApiKey, null);
            var eventConfirmations = (await mainClient.EventConfirmationsAsync(options.ApiKey, null)).Select(Mask).ToList();
            var riderProfiles = (await mainClient.RiderProfilesAsync(options.ApiKey, null)).Select(Mask).ToList();
            var riderRegistrations = await mainClient.RiderRegistrationsAsync(options.ApiKey, null);
            var riderDisqualifications = await mainClient.RiderDisqualificationsAsync(options.ApiKey);
            
            Save(nameof(mainClient.SeriesAsync), series);
            Save(nameof(mainClient.ChampionshipsAsync), championships);
            Save(nameof(mainClient.ClassesAsync), classes);
            Save(nameof(mainClient.EventsAsync), events);
            Save(nameof(mainClient.EventPricesAsync), eventPrices);
            Save(nameof(mainClient.SchedulesAsync), schedules);
            Save(nameof(mainClient.ScheduleToClassAsync), scheduleToClass);
            Save(nameof(mainClient.EventConfirmationsAsync), eventConfirmations);
            Save(nameof(mainClient.RiderProfilesAsync), riderProfiles);
            Save(nameof(mainClient.RiderRegistrationsAsync), riderRegistrations);
            Save(nameof(mainClient.RiderDisqualificationsAsync), riderDisqualifications);
            Console.WriteLine($"Done");
            
            var messageHub = new ChannelMessageHub();
            var storageService = new StorageService(Options.Create(new StorageServiceOptions
            {
                StorageConnectionString = "upstream-data.litedb"
            }), new ChannelMessageHub());
            var upstreamDataStorage = new UpstreamDataRepository(storageService);
            var fakeMainClient = Substitute.For<IMainClient>();
            fakeMainClient.SeriesAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(series);
            fakeMainClient.ChampionshipsAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(championships);
            fakeMainClient.ClassesAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(classes);
            fakeMainClient.EventsAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(events);
            fakeMainClient.EventPricesAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(eventPrices);
            fakeMainClient.SchedulesAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(schedules);
            fakeMainClient.ScheduleToClassAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(scheduleToClass);
            fakeMainClient.EventConfirmationsAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(eventConfirmations);
            fakeMainClient.RiderProfilesAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(riderProfiles);
            fakeMainClient.RiderRegistrationsAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>()).Returns(riderRegistrations);
            fakeMainClient.RiderDisqualificationsAsync(Arg.Any<string>()).Returns(riderDisqualifications);
            var upstreamDataSyncService = new UpstreamDataSyncService(Options.Create(options), fakeMainClient, 
                upstreamDataStorage, messageHub);
            var downloadResult = await upstreamDataSyncService.Download(true);
            
            Console.WriteLine($"Save to litedb = {downloadResult}");
        }

        static readonly Bogus.DataSets.Name names = new("ru");
        static readonly Bogus.DataSets.Address address = new("ru");
        static readonly Bogus.DataSets.Date date = new("ru");
        static RiderProfile Mask(RiderProfile item)
        {
            var name = names.FullName().Split(' ');
            item.Birthdate = date.Past(20);
            item.FirstName = name[0];
            item.ParentName = "А";
            item.LastName = name[1];
            item.City = address.City();
            item.UserName = item.NormalizedUserName = item.NormalizedEmail = item.Email = $"{name[0]}.{name[1]}@braaap.ru";
            item.PhoneNumber = "1234567890";
            return item;
        }

        static EventPrice Mask(EventPrice item)
        {
            item.BasePrice = 0;
            item.PaymentMultiplier = 0;
            return item;
        }
        
        static EventConfirmation Mask(EventConfirmation item)
        {
            item.Paid = 0;
            return item;
        }

        static void Save<T>(string name, ICollection<T> items)
        {
            var serializer = JsonSerializer.Create();
            using var sw = new StreamWriter($"{name}.json");
            serializer.Serialize(sw, items);
            Console.WriteLine($"Saved {items.Count} {name}");
        }
    }
}