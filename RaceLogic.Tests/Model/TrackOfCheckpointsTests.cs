using System;
using System.Linq;
using RaceLogic.Model;
using RaceLogic.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Model
{
    public class TrackOfCheckpointsTests
    {
        [Fact]
        public void Simple_start_and_finish()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[30] 12[32] 13[33]
Rating
F11 3 [5  30]
F12 2 [10 32]
F13 2 [15 33]");
            var fc = FinishCriteria.FromDuration(TimeSpan.FromSeconds(30));
            var track = new TrackOfCheckpoints<int>(DateTime.MinValue, fc);
            foreach (var checkpoint in def.Checkpoints)
                track.Append(checkpoint);
            var rating = track.GetSequence().ToList();
            for (var i = 0; i < def.Rating.Count; i++)
            {
                var expected = def.Rating[i];
                var actual = rating[i];
                actual.RiderId.ShouldBe(expected.RiderId);
                actual.Duration.ShouldBe(expected.Duration);
                actual.Finished.ShouldBe(expected.Finished);
                actual.LapsCount.ShouldBe(expected.LapsCount);
            }
        }
    }
}