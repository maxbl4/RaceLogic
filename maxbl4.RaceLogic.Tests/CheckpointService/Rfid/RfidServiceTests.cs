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
        public RfidServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Should_persist_checkpoints()
        {
            WithRfidService((storageService, rfidService) =>
            {
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                var connectionString = storageService.GetRfidOptions().GetConnectionString();
                connectionString.Protocol.ShouldBe(ReaderProtocolType.Alien);
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                SystemClock.Advance();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});

                var cps = storageService.ListCheckpoints();
                cps.Count.ShouldBe(2);
            });
        }

        [Fact]
        public void Should_store_checkpoint_before_emitting_to_observable()
        {
            WithRfidService((storageService, rfidService) =>
            {
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                List<Checkpoint> cps = null;
                MessageHub.Subscribe<Checkpoint>(x => cps = storageService.ListCheckpoints());
                
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                
                new Timing().Logger(Logger).Expect(() => cps != null);
                cps.Count.ShouldBe(1);
                cps[0].RiderId.ShouldBe("1");
            });
        }
        
        [Fact]
        public void Should_observe_rfid_options_changes()
        {
            WithRfidService((storageService, rfidService) =>
            {
                var cps = new List<Checkpoint>();
                MessageHub.Subscribe<Checkpoint>(x => cps.Add(x));
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                // Rfid enabled, gather checkpoints with '1'
                tagListHandler.ReturnContinuos(new Tag {TagId = "1"});
                new Timing().Logger(Logger).Context("No Tag 1").Expect(() => cps.Count(x => x.RiderId == "1") > 0);

                // Disable Rfid, wait for requests to stop for a second
                storageService.UpdateRfidOptions(o => o.Enabled = false);
                new Timing().Logger(Logger).Context("Did not stop rfid query").Expect(() => 
                    tagListHandler.TimeSinceLastRequest > TimeSpan.FromSeconds(1));
                cps.Clear();
                
                // Now return checkpoints with '2', enable rfid and wait for them
                tagListHandler.ReturnContinuos(new Tag {TagId = "2"});
                storageService.UpdateRfidOptions(o => o.Enabled = true);
                new Timing().Logger(Logger).Context("No Tag 2").Expect(() => cps.Count(x => x.RiderId == "2") > 0);
            });
        }

        [Fact]
        public void Should_emit_tag_reading_statistics()
        {
            // How many times per second a tag was read (RPS)
            // Save tags, that have low RPS (configurable)
            
        }
    }
}