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
            cp.Timestamp.ShouldBe(default(DateTime));
        }
        
        [Fact]
        public void Can_parse_checkpoint_with_timestamp()
        {
            var cp = RoundDefParser.ParseCheckpoint("11[04:50]");
            cp.RiderId.ShouldBe(11);
            cp.Timestamp.ShouldBe(default(DateTime) + new TimeSpan(0, 4, 50));
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
            cps[0].Timestamp.ShouldBe(default(DateTime) + new TimeSpan(0, 0, 50));
            cps[1].RiderId.ShouldBe(12);
            cps[1].Timestamp.ShouldBe(default(DateTime) + new TimeSpan(0, 0, 52));
            cps[2].RiderId.ShouldBe(13);
            cps[2].Timestamp.ShouldBe(default(DateTime) + new TimeSpan(0, 0, 57));
        }

        [Fact]
        public void Can_parse_round_def()
        {
            var rd = RoundDefParser.ParseRoundDef(@"Track

11 12 13
# 15 16
# Comment
11 12 13
Rating
F11 L2
12 L2
13 L2");
            rd.Duration.ShouldBe(TimeSpan.Zero);
            rd.Checkpoints.Count.ShouldBe(6);
            rd.Checkpoints[0].RiderId.ShouldBe(11);
            rd.Checkpoints[1].RiderId.ShouldBe(12);
            rd.Checkpoints[2].RiderId.ShouldBe(13);
            rd.Checkpoints[3].RiderId.ShouldBe(11);
            rd.Checkpoints[4].RiderId.ShouldBe(12);
            rd.Checkpoints[5].RiderId.ShouldBe(13);
            rd.Rating.Count.ShouldBe(3);
            rd.Rating[0].RiderId.ShouldBe(11);
            rd.Rating[0].Finished.ShouldBeTrue();
            rd.Rating[0].LapsCount.ShouldBe(2);
            rd.Rating[0].Laps.Count.ShouldBe(0);
            rd.Rating[1].RiderId.ShouldBe(12);
            rd.Rating[1].Finished.ShouldBeFalse();
            rd.Rating[1].LapsCount.ShouldBe(2);
            rd.Rating[1].Laps.Count.ShouldBe(0);
            rd.Rating[2].RiderId.ShouldBe(13);
            rd.Rating[2].LapsCount.ShouldBe(2);
            rd.Rating[2].Laps.Count.ShouldBe(0);
        }
        
        [Fact]
        public void Can_parse_round_def_of_checkpoints_with_duration()
        {
            var rd = RoundDefParser.ParseRoundDef(@"Track 45:01
11[2] 12[3] 13[4]
11[45:2] 12[45:3]
Rating
F11 L2 [2 45:2]
F12 L2 [3 45:3]
F13 L1 [4     ]");
            rd.HasDuration.ShouldBeTrue();
            rd.Duration.ShouldBe(new TimeSpan(0, 45, 1));
            rd.Checkpoints.Count.ShouldBe(5);
            rd.Checkpoints[0].RiderId.ShouldBe(11);
            rd.Checkpoints[0].Timestamp.ShouldBe(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Checkpoints[1].RiderId.ShouldBe(12);
            rd.Checkpoints[1].Timestamp.ShouldBe(new DateTime(1, 1, 1, 0, 0, 3));
            rd.Checkpoints[3].RiderId.ShouldBe(11);
            rd.Checkpoints[3].Timestamp.ShouldBe(new DateTime(1, 1, 1, 0, 45, 2));
            rd.Rating.Count.ShouldBe(3);
            rd.Rating[0].RiderId.ShouldBe(11);
            rd.Rating[0].LapsCount.ShouldBe(2);
            rd.Rating[0].Laps.Count.ShouldBe(2);
            rd.Rating[0].Laps[0].SequentialNumber.ShouldBe(1);
            rd.Rating[0].Laps[0].Start.ShouldBe(default(DateTime));
            rd.Rating[0].Laps[0].End.ShouldBe(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Rating[0].Laps[1].SequentialNumber.ShouldBe(2);
            rd.Rating[0].Laps[1].Start.ShouldBe(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Rating[0].Laps[1].End.ShouldBe(new DateTime(1, 1, 1, 0, 45, 2));
            
            rd.Rating[2].RiderId.ShouldBe(13);
            rd.Rating[2].LapsCount.ShouldBe(1);
            rd.Rating[2].Laps.Count.ShouldBe(1);
        }

        [Fact]
        public void Should_convert_to_string_and_back()
        {
            var str = @"Track 30
11[5] 12[7] 13[12]
11[40]
Rating
F11 L2 [5 40]
12 L1 [7]
13 L1 [12]";
            var rd = RoundDef.Parse(str);
            rd.ToString().ShouldBe(str);
        }

        [Fact]
        public void Should_format_checkpoints_by_laps()
        {
            var cps = RoundDefParser.ParseCheckpoints("11 12 13 11 12 12 13 11");
            cps.FormatCheckpoints().ShouldBe(@"
11 12 13
11 12
12 13 11");
        }
    }
}