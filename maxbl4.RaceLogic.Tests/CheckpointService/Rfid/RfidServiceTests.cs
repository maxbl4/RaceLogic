using System;
using System.Linq;
using System.Collections.Generic;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
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
        public void Should_persist_checkpoints()
        {
            WithStorageService(storageService =>
            {
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                var rfidService = new RfidService(storageService, MessageHub, systemClock,
                    new NullLogger<RfidService>());
                var connectionString = storageService.GetRfidOptions().GetConnectionString();
                connectionString.Protocol.ShouldBe(ReaderProtocolType.Alien);
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                systemClock.Advance();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});

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
                MessageHub.Subscribe<Checkpoint>(x => cps = storageService.ListCheckpoints());
                var rfidService = new RfidService(storageService, MessageHub, systemClock,
                    new NullLogger<RfidService>());
                
                
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                
                
                Timing.StartWait(() => cps != null).Result.ShouldBeTrue();
                cps.Count.ShouldBe(1);
                cps[0].RiderId.ShouldBe("1");
            });
        }
        
        [Fact]
        public void Should_observe_rfid_options_changes()
        {
            WithStorageService(storageService =>
            {
                var cps = new List<Checkpoint>();
                MessageHub.Subscribe<Checkpoint>(x => cps.Add(x));
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                var rfidService = new RfidService(storageService, MessageHub, systemClock,
                    new NullLogger<RfidService>());
                // Rfid enabled, gather checkpoints with '1'
                tagListHandler.ReturnContinuos(new Tag {TagId = "1"});
                Timing.StartWait(() => cps.Count(x => x.RiderId == "1") > 0)
                    .Result.ShouldBeTrue("No Tag 1");

                // Disable Rfid, wait for requests to stop for a second
                storageService.UpdateRfidOptions(o => o.RfidEnabled = false);
                Timing.StartWait(() => 
                    tagListHandler.TimeSinceLastRequest > TimeSpan.FromSeconds(1))
                    .Result.ShouldBeTrue("Did not stop rfid query");
                cps.Clear();
                
                // Now return checkpoints with '2', enable rfid and wait for them
                tagListHandler.ReturnContinuos(new Tag {TagId = "2"});
                storageService.UpdateRfidOptions(o => o.RfidEnabled = true);
                Timing.StartWait(() => cps.Count(x => x.RiderId == "2") > 0)
                    .Result.ShouldBeTrue("No Tag 2");
            });
        }
    }
}