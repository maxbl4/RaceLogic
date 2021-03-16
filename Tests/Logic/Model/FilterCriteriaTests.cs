using System;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.RoundTiming.Serialization;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Model
{
    public class FilterCriteriaTests
    {
        [Fact]
        public void Get_leader_with_forced_finish()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[20] 
11[25]
12[32] 13[33]
Rating
11 3 [5  20 25]
12 2 [10 32]
13 2 [15 33]");
            var fc = FinishCriteria.FromDuration(TimeSpan.FromSeconds(30));
            fc.GetLeader(def.Rating, false).RiderId.Should().Be("11");
            fc.GetLeader(def.Rating, true).RiderId.Should().Be("12");
        }

        [Fact]
        public void Forced_finish_duration()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[20] 
11[25]
12[32] 13[33]
Rating
11 3 [5  20 25]
12 2 [10 32]
13 2 [15 33]");
            var fc = FinishCriteria.FromDuration(TimeSpan.FromSeconds(30));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeFalse();
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeFalse();
            fc.HasFinished(def.Rating[2], def.Rating, false).Should().BeFalse();

            // With forced finish, the leader is one who has completed round time
            // that is #12
            fc.HasFinished(def.Rating[0], def.Rating, true).Should().BeFalse();
            // #12 finished == true, but we don't store this value
            fc.HasFinished(def.Rating[1], def.Rating, true).Should().BeTrue();
            // Leader was not marker as finished, so #13 is also not finished  
            fc.HasFinished(def.Rating[2], def.Rating, true).Should().BeFalse();

            def.Rating[1].Finish();
            fc.HasFinished(def.Rating[2], def.Rating, true).Should().BeTrue();
        }

        [Fact]
        public void Normal_finish_duration()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[30] 12[32] 13[33]
Rating
11 2 [5  30]
12 2 [10 32]
13 2 [15 33]");
            var fc = FinishCriteria.FromDuration(TimeSpan.FromSeconds(30));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeTrue();
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeFalse();
            fc.HasFinished(def.Rating[2], def.Rating, false).Should().BeFalse();
            def.Rating[0].Finish();

            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeTrue();
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeTrue();
            fc.HasFinished(def.Rating[2], def.Rating, false).Should().BeTrue();
        }

        [Fact]
        public void Normal_finish_duration_with_additional_laps()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]
11[30]
Rating
11 2 [5 31]");
            var fc = FinishCriteria.FromDuration(TimeSpan.FromSeconds(30), 1);
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeFalse();
            def.Rating[0].Append(new Checkpoint("11", DateTime.MinValue + TimeSpan.FromSeconds(40)));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeTrue();
        }

        [Fact]
        public void Finish_by_lap_count()
        {
            var def = RoundDef.Parse(@"Track 30
11[5] 12[6]
11[30]
Rating
11 2 [5 31]
12 1 [6]");
            var fc = FinishCriteria.FromTotalLaps(3, TimeSpan.FromSeconds(50));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeFalse();
            def.Rating[0].Append(new Checkpoint("11", DateTime.MinValue + TimeSpan.FromSeconds(40)));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeTrue();

            def.Rating[0].Finish();
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeFalse();
            def.Rating[1].Append(new Checkpoint("12", DateTime.MinValue + TimeSpan.FromSeconds(60)));
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeTrue();
        }

        [Fact]
        public void Finish_by_lap_count_skip_first_lap()
        {
            var def = RoundDef.Parse(@"Track 30
11[5] 12[6]
11[30]
Rating
11 2 [5 31]
12 1 [6]");
            var fc = FinishCriteria.FromTotalLaps(2, TimeSpan.FromSeconds(50), true);
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeFalse();
            def.Rating[0].Append(new Checkpoint("11", DateTime.MinValue + TimeSpan.FromSeconds(40)));
            fc.HasFinished(def.Rating[0], def.Rating, false).Should().BeTrue();

            def.Rating[0].Finish();
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeFalse();
            def.Rating[1].Append(new Checkpoint("12", DateTime.MinValue + TimeSpan.FromSeconds(60)));
            fc.HasFinished(def.Rating[1], def.Rating, false).Should().BeTrue();
        }
    }
}