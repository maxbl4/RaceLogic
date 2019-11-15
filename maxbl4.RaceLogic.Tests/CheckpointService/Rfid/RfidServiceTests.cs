using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Easy.MessageHub;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Rfid
{
    public class RfidServiceTests : IntegrationTestBase
    {
        private readonly FakeSystemClock systemClock;
        private readonly TagListHandler tagListHandler;

        public RfidServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            systemClock = new FakeSystemClock();
            tagListHandler = WithStorageService(storageService => new SimulatorBuilder(storageService).Build());
        }

        [Fact]
        public async Task Should_persist_checkpoints()
        {
            WithStorageService(storageService =>
            {
                var rfidService = new RfidService(storageService, new MessageHub(), systemClock,
                    new NullLogger<RfidService>());
                var connectionString = storageService.GetRfidOptions().GetConnectionString();
                connectionString.Protocol.ShouldBe(ReaderProtocolType.Alien);
                tagListHandler.ReturnOnce(new Tag {TagId = "1"}).Wait(1000);
                systemClock.Advance();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"}).Wait(1000);

                var cps = storageService.ListCheckpoints();
                cps.Count.ShouldBe(2);
            });
        }

        [Fact]
        public void Should_store_checkpoint_before_emitting_to_observable()
        {
            throw new NotImplementedException();
        }
    }
}