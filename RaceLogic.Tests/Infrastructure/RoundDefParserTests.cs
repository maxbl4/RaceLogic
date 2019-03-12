using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Infrastructure
{
    public class RoundDefParserTests
    {
        [Fact]
        public void Can_parse_checkpoint_without_timestamp()
        {
            var cp = RoundDefParser.ParseCheckpoint("11");
            cp.RiderId.ShouldBe(11);
            cp.Timestamp.ShouldBe(DateTimeOffset.MinValue);
        }
        
        [Fact]
        public void Can_parse_checkpoint_with_timestamp()
        {
            var cp = RoundDefParser.ParseCheckpoint("11[04:50]");
            cp.RiderId.ShouldBe(11);
            cp.Timestamp.ShouldBe(DateTimeOffset.MinValue + new TimeSpan(0, 4, 50));
        }
        
        [Fact]
        public void Can_parse_checkpoints_line_without_timestamps()
        {
            var cps = RoundDefParser.ParseCheckpoints("11 \t12, 13").ToList();
            cps.Count.ShouldBe(3);
            cps[0].RiderId.ShouldBe(11);
            cps[1].RiderId.ShouldBe(12);
            cps[2].RiderId.ShouldBe(13);
        }
        
        [Fact]
        public void Can_parse_checkpoints_line_with_timestamps()
        {
            var cps = RoundDefParser.ParseCheckpoints("11[50] 12[52] 13[57]").ToList();
            cps.Count.ShouldBe(3);
            cps[0].RiderId.ShouldBe(11);
            cps[0].Timestamp.ShouldBe(DateTimeOffset.MinValue + new TimeSpan(0, 0, 50));
            cps[1].RiderId.ShouldBe(12);
            cps[1].Timestamp.ShouldBe(DateTimeOffset.MinValue + new TimeSpan(0, 0, 52));
            cps[2].RiderId.ShouldBe(13);
            cps[2].Timestamp.ShouldBe(DateTimeOffset.MinValue + new TimeSpan(0, 0, 57));
        }

        [Fact]
        public void Can_parse_round_def_of_checkpoints()
        {
            var rd = RoundDefParser.ParseRoundDef(@"Track

11 12 13
# 15 16
# Comment
11 12 13");
            rd.Duration.ShouldBe(TimeSpan.Zero);
            rd.Checkpoints.Count.ShouldBe(6);
            rd.Checkpoints[0].RiderId.ShouldBe(11);
            rd.Checkpoints[1].RiderId.ShouldBe(12);
            rd.Checkpoints[2].RiderId.ShouldBe(13);
            rd.Checkpoints[3].RiderId.ShouldBe(11);
            rd.Checkpoints[4].RiderId.ShouldBe(12);
            rd.Checkpoints[5].RiderId.ShouldBe(13);
        }
        
        [Fact]
        public void Can_parse_round_def_of_checkpoints_with_duration()
        {
            var rd = RoundDefParser.ParseRoundDef(@"Track 45:01
11 12 13");
            rd.Duration.ShouldBe(new TimeSpan(0, 45, 1));
            rd.Checkpoints.Count.ShouldBe(3);
            rd.Checkpoints[0].RiderId.ShouldBe(11);
            rd.Checkpoints[1].RiderId.ShouldBe(12);
            rd.Checkpoints[2].RiderId.ShouldBe(13);
        }
    }
}