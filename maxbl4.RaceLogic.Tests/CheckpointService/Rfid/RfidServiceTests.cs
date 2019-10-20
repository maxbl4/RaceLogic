using System.Threading;
using Easy.MessageHub;
using maxbl4.RfidCheckpointService.Rfid;
using maxbl4.RfidDotNet;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Rfid
{
    public class RfidServiceTests : StorageServiceFixture
    {
        readonly RfidService rfidService;
        private readonly FakeSystemClock systemClock;
        private readonly FakeUniversalTagStream tagStream;

        public RfidServiceTests()
        {
            systemClock = new FakeSystemClock();
            tagStream = new FakeUniversalTagStream();
            rfidService = new RfidService(storageService, new MessageHub(), systemClock, new NullLogger<RfidService>(),cs => tagStream);
        }

        [Fact]
        public void Should_persist_checkpoints()
        {
            storageService.SetRfidSettings(new RfidSettings{RfidEnabled = true, SerializedConnectionString = "protocol=fake"});
            var connectionString = storageService.GetRfidSettings().GetConnectionString();
            connectionString.Protocol.ShouldBe(ReaderProtocolType.Fake);
            rfidService.StartAsync(CancellationToken.None).Wait();
            tagStream.TagsSubject.OnNext(new Tag{TagId = "1"});
            systemClock.Advance();
            tagStream.TagsSubject.OnNext(new Tag{TagId = "1"});

            var cps = storageService.ListCheckpoints();
            cps.Count.ShouldBe(2);
        }
    }
}