using System;
using System.Collections.Generic;
using FluentAssertions;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic.Checkpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;
using Tag = maxbl4.Race.CheckpointService.Model.Tag;

namespace maxbl4.Race.Tests.CheckpointService.Storage
{
    public class StorageServiceTests : IntegrationTestBase
    {
        public StorageServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Should_store_and_load_utc_date()
        {
            WithCheckpointStorageService(s =>
            {
                var dt = DateTime.UtcNow;
                s.AppendCheckpoint(new Checkpoint("111", dt));
                s.ListCheckpoints()[0].Timestamp.Should().BeCloseTo(dt, TimeSpan.FromSeconds(1));
            });
        }

        [Fact]
        public void Should_save_and_load_rfidsettings()
        {
            WithCheckpointStorageService(storageService =>
            {
                var settings = storageService.GetRfidOptions();
                settings.ConnectionString.Should().Be(RfidOptions.DefaultConnectionString);
                settings.Enabled = true;
                settings.ConnectionString = "Protocol=Alien;Network=8.8.8.8:500";
                storageService.SetRfidOptions(settings);
                settings = storageService.GetRfidOptions();
                settings.Should().NotBeSameAs(RfidOptions.Default);
                settings.Enabled.Should().BeTrue();
                settings.ConnectionString.Should().Be("Protocol=Alien;Network=8.8.8.8:500");
            });
        }

        [Fact]
        public void Should_return_default_rfidsettings()
        {
            WithCheckpointStorageService(storageService =>
            {
                var settings = storageService.GetRfidOptions();
                settings.ConnectionString.Should().Be(RfidOptions.DefaultConnectionString);
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
            settings.Should().BeSameAs(opts);
        }
        
        [Fact]
        public void Should_support_date_filter()
        {
            WithCheckpointStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                storageService.AppendCheckpoint(new Checkpoint("1", ts));
                storageService.AppendCheckpoint(new Checkpoint("2", ts.AddSeconds(100)));
                storageService.ListCheckpoints().Count.Should().Be(2);
                var firstCp = storageService.ListCheckpoints(ts, ts.AddSeconds(10));
                firstCp.Count.Should().Be(1);
                firstCp[0].RiderId.Should().Be("1");

                var secondCp = storageService.ListCheckpoints(ts.AddSeconds(10));
                secondCp.Count.Should().Be(1);
                secondCp[0].RiderId.Should().Be("2");
            });
        }
        
        [Fact]
        public void Should_update_timestamp()
        {
            WithCheckpointStorageService(storageService =>
            {
                var options = storageService.GetRfidOptions();
                options.Timestamp.Should().Be(default);
                storageService.SetRfidOptions(options);
                storageService.GetRfidOptions().Timestamp.Should().Be(SystemClock.UtcNow.UtcDateTime);
            });
        }
        
        [Fact]
        public void Should_support_tag_operations()
        {
            WithCheckpointStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                for (var i = 1; i <= 10; i++)
                {
                    storageService.AppendTag(new Tag{Antenna = i, Rssi = i, DiscoveryTime = ts.AddSeconds(i), LastSeenTime = ts.AddSeconds(i + 1), ReadCount = i, TagId = i.ToString()});
                }
                var list = storageService.ListTags();
                list.Count.Should().Be(10);
                for (var i = 1; i <= 10; i++)
                {
                    var t = list[10 - i];
                    t.Antenna.Should().Be(i);
                    t.Rssi.Should().Be(i);
                    t.DiscoveryTime.Should().BeCloseTo(ts.AddSeconds(i), TimeSpan.FromMilliseconds(10));
                    t.LastSeenTime.Should().BeCloseTo(ts.AddSeconds(i + 1), TimeSpan.FromMilliseconds(10));
                    t.ReadCount.Should().Be(i);
                    t.TagId.Should().Be(i.ToString());
                }

                storageService.DeleteTags(ts.AddSeconds(-1), ts.AddSeconds(7.7)).Should().Be(7);
                
                list = storageService.ListTags();
                list.Count.Should().Be(3);
                for (var i = 8; i <= 10; i++)
                {
                    var t = list[10 - i];
                    t.Antenna.Should().Be(i);
                    t.Rssi.Should().Be(i);
                    t.DiscoveryTime.Should().BeCloseTo(ts.AddSeconds(i), TimeSpan.FromMilliseconds(10));
                    t.LastSeenTime.Should().BeCloseTo(ts.AddSeconds(i + 1), TimeSpan.FromMilliseconds(10));
                    t.ReadCount.Should().Be(i);
                    t.TagId.Should().Be(i.ToString());
                }
            });
        }
        
        [Fact]
        public void Should_save_and_load_tags_with_same_tagId()
        {
            WithCheckpointStorageService(storageService =>
            {
                var ts = DateTime.UtcNow;
                storageService.AppendTag(new Tag{Antenna = 1, DiscoveryTime = ts, TagId = "1"});
                storageService.AppendTag(new Tag{Antenna = 2, DiscoveryTime = ts, TagId = "1"});
                var list = storageService.ListTags();
                list.Count.Should().Be(2);
                list.Should().Contain(x => x.Antenna == 1);
                list.Should().Contain(x => x.Antenna == 2);
            });
        }

        [Fact]
        public void Should_load_initial_rfid_options_from_settings()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("ServiceOptions:InitialRfidOptions:ConnectionString", "Protocol=Alien;Network=sim:20023");
            dict.Add("ServiceOptions:InitialRfidOptions:PersistTags", "true");
            dict.Add("ServiceOptions:InitialRfidOptions:CheckpointAggregationWindowMs", "1000");
            dict.Add("ServiceOptions:InitialRfidOptions:RpsThreshold", "1000");
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
            var opts = config.GetSection(nameof(ServiceOptions))
                .Get<ServiceOptions>();
            opts.InitialRfidOptions.Should().NotBeNull();
            opts.InitialRfidOptions.Enabled.Should().BeFalse();
            opts.InitialRfidOptions.PersistTags.Should().BeTrue();
        }

        [Fact]
        public void Should_rotate_database_in_case_of_validation_failure()
        {
            var cs = new LiteDB.ConnectionString(storageConnectionString);
            var dbFile = new RollingFileInfo(cs.Filename);
            dbFile.BaseExists.Should().BeFalse();
            using (var repo = new LiteRepository(storageConnectionString))
            {
                repo.Insert(new BadCheckpoint{Count = 1.1m}, nameof(Checkpoint));
            }
            dbFile.BaseExists.Should().BeTrue();
            dbFile.Index.Should().Be(0);

            WithCheckpointStorageService(storage =>
            {
                dbFile.Index.Should().Be(1);
            });
        }

        class BadCheckpoint
        {
            public long Id { get; set; }
            public decimal Count { get; set; }
        }
    }
}