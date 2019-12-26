using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.CheckpointService.Rfid
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
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                new Timing()
                    .FailureDetails(() => storageService.ListCheckpoints().Count.ToString())
                    .Expect(() => storageService.ListCheckpoints().Count(y => !y.Aggregated) == 1);
                // Need to wait before advance, to prevent race between Advance and checkpoint code in RfidService
                SystemClock.Advance();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                new Timing()
                    .FailureDetails(() => storageService.ListCheckpoints().Count.ToString())
                    .Expect(() => storageService.ListCheckpoints().Count(y => !y.Aggregated) == 2);
            });
        }
        
        [Fact]
        public void Should_persist_tags_and_checkpoints()
        {
            WithRfidService((storageService, rfidService) =>
            {
                storageService.UpdateRfidOptions(x => x.PersistTags = true);
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                new Timing()
                    .FailureDetails(() => storageService.ListCheckpoints().Count.ToString())
                    .Expect(() => storageService.ListCheckpoints().Count(y => !y.Aggregated) == 1);
                // Need to wait before advance, to prevent race between Advance and checkpoint code in RfidService
                SystemClock.Advance();
                tagListHandler.ReturnOnce(new Tag {TagId = "1"});
                new Timing()
                    .FailureDetails(() => storageService.ListCheckpoints().Count.ToString())
                    .Expect(() => storageService.ListCheckpoints().Count(y => !y.Aggregated) == 2);
                
                new Timing()
                    .FailureDetails(() => storageService.ListTags().Count.ToString())
                    .Expect(() => storageService.ListTags().Count == 2);
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
            WithRfidService((storageService, rfidService) =>
            {
                SystemClock.UseRealClock();
                var cps = new List<Checkpoint>();
                MessageHub.Subscribe<Checkpoint>(x => cps.Add(x));
                var tagListHandler = new SimulatorBuilder(storageService).Build();
                tagListHandler.ReturnContinuos(new Tag {TagId = "1"});
                new Timing().Logger(Logger)
                    .Expect(() => cps.Any(x => x.RiderId == "1"
                            && x.Aggregated && x.Count > 1));
            });
        }

        [Fact]
        public void Should_disable_stale_rfid()
        {
            WithCheckpointStorageService(s =>
            {
                s.UpdateRfidOptions(o => o.Enabled = true);
            });
            SystemClock.Advance(TimeSpan.FromDays(1.2));
            WithRfidService((s, r) =>
            {
                var o = s.GetRfidOptions();
                o.Enabled.ShouldBeFalse();
                o.Timestamp.ShouldBe(SystemClock.UtcNow.UtcDateTime);
            });
        }
    }
}