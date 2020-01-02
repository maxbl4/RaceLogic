using FluentAssertions;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.RoundTiming.Serialization;

namespace maxbl4.Race.Tests.Infrastructure
{
    public static class RoundDefExt
    {
        public static void VerifyTrack(this RoundDef def, ITrackOfCheckpoints track, bool verifyTime = true)
        {
            var rating = track.Rating;
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
                actual.LapCount.Should().Be(expected.LapCount,
                    $"#{actual.RiderId} should have LapsCount={expected.LapCount}, but was {actual.LapCount}");
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