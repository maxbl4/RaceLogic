using System;
using System.Collections.Generic;
using System.IO;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Storage
{
    public class StorageServiceTests : IntegrationTestBase
    {
        public StorageServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Should_store_and_load_utc_date()
        {
            WithStorageService(s =>
            {
                var dt = DateTime.UtcNow;
                s.AppendCheckpoint(new Checkpoint("111", dt));
                s.ListCheckpoints()[0].Timestamp.ShouldBe(dt, TimeSpan.FromSeconds(1));
            });
        }

        [Fact]
        public void Should_save_and_load_rfidsettings()
        {
            WithStorageService(storageService =>
            {
                var settings = storageService.GetRfidOptions();
                settings.ConnectionString.ShouldBe(RfidOptions.DefaultConnectionString);
                settings.Enabled = true;
                settings.ConnectionString = "Protocol=Alien;Network=8.8.8.8:500";
                storageService.SetRfidOptions(settings);
                settings = storageService.GetRfidOptions();
                settings.ShouldNotBeSameAs(RfidOptions.Default);
                settings.Enabled.ShouldBeTrue();
                settings.ConnectionString.ShouldBe("Protocol=Alien;Network=8.8.8.8:500");
            });
        }

        [Fact]
        public void Should_return_default_rfidsettings()
        {
            WithStorageService(storageService =>
            {
                var settings = storageService.GetRfidOptions();
                settings.ConnectionString.ShouldBe(RfidOptions.DefaultConnectionString);
            });
        }
        
        [Fact]
        public void Should_return_initial_rfid_options()
        {
            var opts = new RfidOptions();
            using var storageService = new StorageService(Options
                .Create(new ServiceOptions
                {
                    StorageConnectionString = storageConnectionString,
                    InitialRfidOptions = opts
                }), MessageHub, SystemClock);
            
            var settings = storageService.GetRfidOptions();
            settings.ShouldBeSameAs(opts);
        }
        
        [Fact]
        public void Should_support_date_filter()
        {
            WithStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                storageService.AppendCheckpoint(new Checkpoint("1", ts));
                storageService.AppendCheckpoint(new Checkpoint("2", ts.AddSeconds(100)));
                storageService.ListCheckpoints().Count.ShouldBe(2);
                var firstCp = storageService.ListCheckpoints(ts, ts.AddSeconds(10));
                firstCp.Count.ShouldBe(1);
                firstCp[0].RiderId.ShouldBe("1");

                var secondCp = storageService.ListCheckpoints(ts.AddSeconds(10));
                secondCp.Count.ShouldBe(1);
                secondCp[0].RiderId.ShouldBe("2");
            });
        }
        
        [Fact]
        public void Should_update_timestamp()
        {
            WithStorageService(storageService =>
            {
                var options = storageService.GetRfidOptions();
                options.Timestamp.ShouldBe(default);
                storageService.SetRfidOptions(options);
                storageService.GetRfidOptions().Timestamp.ShouldBe(SystemClock.UtcNow.UtcDateTime);
            });
        }
        
        [Fact]
        public void Should_support_tag_operations()
        {
            WithStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                for (var i = 1; i <= 10; i++)
                {
                    storageService.AppendTag(new Tag{Antenna = i, Rssi = i, DiscoveryTime = ts.AddSeconds(i), LastSeenTime = ts.AddSeconds(i + 1), ReadCount = i, TagId = i.ToString()});
                }
                var list = storageService.ListTags();
                list.Count.ShouldBe(10);
                for (var i = 1; i <= 10; i++)
                {
                    var t = list[i - 1];
                    t.Antenna.ShouldBe(i);
                    t.Rssi.ShouldBe(i, 0.01);
                    t.DiscoveryTime.ShouldBe(ts.AddSeconds(i), TimeSpan.FromMilliseconds(10));
                    t.LastSeenTime.ShouldBe(ts.AddSeconds(i + 1), TimeSpan.FromMilliseconds(10));
                    t.ReadCount.ShouldBe(i);
                    t.TagId.ShouldBe(i.ToString());
                }

                storageService.DeleteTags(ts.AddSeconds(-1), ts.AddSeconds(7.7)).ShouldBe(7);
                
                list = storageService.ListTags();
                list.Count.ShouldBe(3);
                for (var i = 8; i <= 10; i++)
                {
                    var t = list[i - 8];
                    t.Antenna.ShouldBe(i);
                    t.Rssi.ShouldBe(i, 0.01);
                    t.DiscoveryTime.ShouldBe(ts.AddSeconds(i), TimeSpan.FromMilliseconds(10));
                    t.LastSeenTime.ShouldBe(ts.AddSeconds(i + 1), TimeSpan.FromMilliseconds(10));
                    t.ReadCount.ShouldBe(i);
                    t.TagId.ShouldBe(i.ToString());
                }
            });
        }
        
        [Fact]
        public void Should_save_and_load_tags_with_same_tagId()
        {
            WithStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                storageService.AppendTag(new Tag{Antenna = 1, DiscoveryTime = ts, TagId = "1"});
                storageService.AppendTag(new Tag{Antenna = 2, DiscoveryTime = ts, TagId = "1"});
                var list = storageService.ListTags();
                list.Count.ShouldBe(2);
                list.ShouldContain(x => x.Antenna == 1);
                list.ShouldContain(x => x.Antenna == 2);
            });
        }

        [Fact]
        public void Should_load_initial_rfid_options_from_settings()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("ServiceOptions:InitialRfidOptions:ConnectionString", "Protocol=Alien;Network=sim:20023");
            //dict.Add("ServiceOptions:InitialRfidOptions:Enabled", "true");
            dict.Add("ServiceOptions:InitialRfidOptions:PersistTags", "true");
            dict.Add("ServiceOptions:InitialRfidOptions:CheckpointAggregationWindowMs", "1000");
            dict.Add("ServiceOptions:InitialRfidOptions:RpsThreshold", "1000");
            //dict.Add("ServiceOptions:StorageConnectionString", "aaa");
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
            var opts = config.GetSection(nameof(ServiceOptions))
                .Get<ServiceOptions>();
            //opts.StorageConnectionString.ShouldBe("aaa");
            opts.InitialRfidOptions.ShouldNotBeNull();
            opts.InitialRfidOptions.Enabled.ShouldBeFalse();
            opts.InitialRfidOptions.PersistTags.ShouldBeTrue();
        }
    }
}