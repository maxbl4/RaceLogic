using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Tests.Infrastructure
{
    public static class RoundDefExt
    {
        public static TrackOfCheckpoints CreateTrack(this RoundDef def, FinishCriteria fc)
        {
            var track = new TrackOfCheckpoints(def.RoundStartTime, fc);
            foreach (var checkpoint in def.Checkpoints)
                track.Append(checkpoint);
            return track;
        }

        public static void VerifyTrack(this RoundDef def, TrackOfCheckpoints track, bool verifyTime = true)
        {
            var rating = track.GetSequence().ToList();
            for (var i = 0; i < def.Rating.Count; i++)
            {
                var expected = def.Rating[i];
                var actual = rating[i];
                actual.RiderId.Should().Be(expected.RiderId, 
                    $"Place {i + 1} should have #{expected.RiderId}, but was #{actual.RiderId}");
                actual.Started.Should().Be(expected.Started,
                    $"#{actual.RiderId} should have Started={expected.Started}, but was {actual.Started}");
                actual.Finished.Should().Be(expected.Finished,
                    $"#{actual.RiderId} should have Finished={expected.Finished}, but was {actual.Finished}");
                actual.LapsCount.Should().Be(expected.LapsCount,
                    $"#{actual.RiderId} should have LapsCount={expected.LapsCount}, but was {actual.LapsCount}");
                if (verifyTime)
                {
                    actual.Start.Should().Be(def.RoundStartTime,
                        $"#{actual.RiderId} should have Start={expected.Start}, but was {actual.Start}");

                    actual.Duration.Should().Be(expected.Duration,
                        $"#{actual.RiderId} should have Duration={expected.Duration}, but was {actual.Duration}");
                }
            }
        }
    }
}