using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Easy.MessageHub;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.ReaderSimulator;
using maxbl4.RfidDotNet.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Rfid
{
    public class RfidServiceTests : IntegrationTestBase
    {
        private readonly FakeSystemClock systemClock;

        public RfidServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            systemClock = new FakeSystemClock();
        }

        [Fact]
        public async Task Should_persist_checkpoints()
        {
            WithStorageService(storageService =>
            {
                var tagListHandler = new SimulatorBuilder(storageService).Build();
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
            WithStorageService(storageService =>
            {
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                List<Checkpoint> cps = null;
                var messageHub = new MessageHub();
                messageHub.Subscribe<Checkpoint>(x => cps = storageService.ListCheckpoints());
                var rfidService = new RfidService(storageService, messageHub, systemClock,
                    new NullLogger<RfidService>());
                
                
                tagListHandler.ReturnOnce(new Tag {TagId = "1"}).Wait(1000);
                
                
                Timing.StartWait(() => cps != null).Result.ShouldBeTrue();
                cps.Count.ShouldBe(1);
                cps[0].RiderId.ShouldBe("1");
            });
        }
    }
}