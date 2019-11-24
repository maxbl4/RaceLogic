using System;
using System.IO;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Services;
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
    }
}