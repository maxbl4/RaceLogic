using maxbl4.RfidCheckpointService.Rfid;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Rfid
{
    public class RfidServiceTests : StorageServiceFixture
    {
        RfidService rfidService;
        private FakeSystemClock systemClock;

        public RfidServiceTests()
        {
            systemClock = new FakeSystemClock();
            rfidService = new RfidService(storageService, systemClock);
        }

        [Fact]
        public void Should()
        {
            storageService.SetRfidSettings(new RfidSettings());
        }
    }
}