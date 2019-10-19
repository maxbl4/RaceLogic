using System.IO;
using maxbl4.RfidCheckpointService.Rfid;
using maxbl4.RfidCheckpointService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Storage
{
    public class StorageServiceTests : StorageServiceFixture
    {
        private readonly StorageService storageService;

        public StorageServiceTests()
        {
            storageService = new StorageService(connectionString);   
        }

        [Fact]
        public void Should_save_and_load_rfidsettings()
        {
            var settings = storageService.GetRfidSettings();
            settings.ShouldBeSameAs(RfidSettings.Default);
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
            settings.ShouldBeSameAs(RfidSettings.Default);
        }
    }
}