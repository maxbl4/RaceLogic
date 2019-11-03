using System;
using System.IO;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Storage
{
    public class StorageServiceTests : StorageServiceFixture
    {
        [Fact]
        public void Should_save_and_load_rfidsettings()
        {
            var settings = storageService.GetRfidSettings();
            settings.ShouldBeSameAs(RfidOptions.Default);
            settings.RfidEnabled = true;
            settings.SerializedConnectionString = "Protocol=Alien;Network=8.8.8.8:500";
            storageService.SetRfidSettings(settings);
            settings = storageService.GetRfidSettings();
            settings.RfidEnabled.ShouldBeTrue();
            settings.SerializedConnectionString.ShouldBe("Protocol=Alien;Network=8.8.8.8:500");
        }

        [Fact]
        public void Should_return_default_rfidsettings()
        {
            var settings = storageService.GetRfidSettings();
            settings.ShouldBeSameAs(RfidOptions.Default);
        }
        
        [Fact]
        public void Should_support_date_filter()
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
        }
    }
}