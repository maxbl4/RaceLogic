using System;
using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic.RoundTiming.Serialization;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class RoundDefParserTests
    {
        [Fact]
        public void Can_parse_track_header()
        {
            var (roundStartTime, duration) = RoundDefParser.ParseTrackHeader("Track 30");
            roundStartTime.Should().Be(DateTime.MinValue);
            duration.Should().Be(TimeSpan.FromSeconds(30));
            (roundStartTime, duration) = RoundDefParser.ParseTrackHeader("Track 2018-05-06T21:30:15 48:16");
            roundStartTime.Should().Be(new DateTime(2018, 5, 6, 21, 30, 15, DateTimeKind.Utc));
            duration.Should().Be(new TimeSpan(0, 48, 16));
        }

        [Fact]
        public void Can_parse_checkpoint_without_timestamp()
        {
            var cp = RoundDefParser.ParseCheckpoint("11", default);
            cp.RiderId.Should().Be("11");
            cp.Timestamp.Should().Be(default);
        }
        
        [Fact]
        public void Can_parse_checkpoint_with_timestamp()
        {
            var cp = RoundDefParser.ParseCheckpoint("11[04:50]", default);
            cp.RiderId.Should().Be("11");
            cp.Timestamp.Should().Be(default(DateTime) + new TimeSpan(0, 4, 50));
        }
        
        [Fact]
        public void Can_parse_checkpoints_line_without_timestamps()
        {
            var cps = RoundDefParser.ParseCheckpoints("11 \t12, 13", default).ToList();
            cps.Count.Should().Be(3);
            cps[0].RiderId.Should().Be("11");
            cps[1].RiderId.Should().Be("12");
            cps[2].RiderId.Should().Be("13");
        }
        
        [Fact]
        public void Can_parse_checkpoints_line_with_timestamps()
        {
            var start = new DateTime(5000);
            var cps = RoundDefParser.ParseCheckpoints("11[50] 12[52] 13[57]", start).ToList();
            cps.Count.Should().Be(3);
            cps[0].RiderId.Should().Be("11");
            cps[0].Timestamp.Should().Be(start + new TimeSpan(0, 0, 50));
            cps[1].RiderId.Should().Be("12");
            cps[1].Timestamp.Should().Be(start + new TimeSpan(0, 0, 52));
            cps[2].RiderId.Should().Be("13");
            cps[2].Timestamp.Should().Be(start + new TimeSpan(0, 0, 57));
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
F11 L2 [1 31]
12 L2 [2 32]
13 L2 [3 33]");
            rd.Duration.Should().Be(TimeSpan.Zero);
            rd.Checkpoints.Count.Should().Be(6);
            rd.Checkpoints[0].RiderId.Should().Be("11");
            rd.Checkpoints[1].RiderId.Should().Be("12");
            rd.Checkpoints[2].RiderId.Should().Be("13");
            rd.Checkpoints[3].RiderId.Should().Be("11");
            rd.Checkpoints[4].RiderId.Should().Be("12");
            rd.Checkpoints[5].RiderId.Should().Be("13");
            rd.Rating.Count.Should().Be(3);
            rd.Rating[0].RiderId.Should().Be("11");
            rd.Rating[0].Finished.Should().BeTrue();
            rd.Rating[0].LapsCount.Should().Be(2);
            rd.Rating[0].Laps.Count.Should().Be(2);
            rd.Rating[1].RiderId.Should().Be("12");
            rd.Rating[1].Finished.Should().BeFalse();
            rd.Rating[1].LapsCount.Should().Be(2);
            rd.Rating[1].Laps.Count.Should().Be(2);
            rd.Rating[2].RiderId.Should().Be("13");
            rd.Rating[2].LapsCount.Should().Be(2);
            rd.Rating[2].Laps.Count.Should().Be(2);
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
            rd.HasDuration.Should().BeTrue();
            rd.Duration.Should().Be(new TimeSpan(0, 45, 1));
            rd.Checkpoints.Count.Should().Be(5);
            rd.Checkpoints[0].RiderId.Should().Be("11");
            rd.Checkpoints[0].Timestamp.Should().Be(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Checkpoints[1].RiderId.Should().Be("12");
            rd.Checkpoints[1].Timestamp.Should().Be(new DateTime(1, 1, 1, 0, 0, 3));
            rd.Checkpoints[3].RiderId.Should().Be("11");
            rd.Checkpoints[3].Timestamp.Should().Be(new DateTime(1, 1, 1, 0, 45, 2));
            rd.Rating.Count.Should().Be(3);
            rd.Rating[0].RiderId.Should().Be("11");
            rd.Rating[0].LapsCount.Should().Be(2);
            rd.Rating[0].Laps.Count.Should().Be(2);
            rd.Rating[0].Laps[0].SequentialNumber.Should().Be(1);
            rd.Rating[0].Laps[0].Start.Should().Be(default);
            rd.Rating[0].Laps[0].End.Should().Be(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Rating[0].Laps[1].SequentialNumber.Should().Be(2);
            rd.Rating[0].Laps[1].Start.Should().Be(new DateTime(1, 1, 1, 0, 0, 2));
            rd.Rating[0].Laps[1].End.Should().Be(new DateTime(1, 1, 1, 0, 45, 2));
            
            rd.Rating[2].RiderId.Should().Be("13");
            rd.Rating[2].LapsCount.Should().Be(1);
            rd.Rating[2].Laps.Count.Should().Be(1);
        }
        
        [Fact]
        public void Can_parse_round_def_of_checkpoints_with_start_time_and_duration()
        {
            var rd = RoundDefParser.ParseRoundDef(@"Track 2018-02-03T04:05:06 45:01
11[2] 12[3] 13[4]
11[45:2] 12[45:3]
Rating
F11 L2 [2 45:2]
F12 L2 [3 45:3]
F13 L1 [4     ]");
            rd.HasDuration.Should().BeTrue();
            rd.Duration.Should().Be(new TimeSpan(0, 45, 1));
            rd.Checkpoints.Count.Should().Be(5);
            rd.Checkpoints[0].RiderId.Should().Be("11");
            rd.Checkpoints[0].Timestamp.Should().Be(new DateTime(2018, 2, 3, 4, 5, 6 + 2));
            rd.Checkpoints[1].RiderId.Should().Be("12");
            rd.Checkpoints[1].Timestamp.Should().Be(new DateTime(2018, 2, 3, 4, 5, 6 + 3));
            rd.Checkpoints[3].RiderId.Should().Be("11");
            rd.Checkpoints[3].Timestamp.Should().Be(new DateTime(2018, 2, 3, 4, 50, 6 + 2));
        }

        [Fact]
        public void Should_parse_positions()
        {
            var pos = RoundDefParser.ParseRating("11", new DateTime(5000));
            pos.RiderId.Should().Be("11");
            pos.Start.Should().Be(DateTime.MinValue);
            pos.Started.Should().BeFalse();
            pos.Finished.Should().BeFalse();
            pos.LapsCount.Should().Be(0);

            Assert.Throws<FormatException>(() => RoundDefParser.ParseRating("11 3", new DateTime(5000)));
            
            pos = RoundDefParser.ParseRating("F11 2 [1 2]", new DateTime(5000));
            pos.RiderId.Should().Be("11");
            pos.Start.Should().Be(new DateTime(5000));
            pos.Started.Should().BeTrue();
            pos.Finished.Should().BeTrue();
            pos.LapsCount.Should().Be(2);
            pos.StartSequence.Should().BeGreaterThan(0);
            pos.EndSequence.Should().BeGreaterThan(pos.StartSequence);
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
            rd.ToString().Should().Be(str);
        }
    }
}