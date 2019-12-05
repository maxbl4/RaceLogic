using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Model
{
    public class LapTests
    {
        [Fact]
        public void Laps_with_timestamps()
        {
            var cp = new Checkpoint("11", new DateTime(2000));
            var l = new Lap(cp, new DateTime(1000));
            l.SequentialNumber.ShouldBe(1);
            l.Start.ShouldBe(new DateTime(1000));
            l.End.ShouldBe(new DateTime(2000));
            l.Duration.ShouldBe(TimeSpan.FromTicks(1000));
            l.AggDuration.ShouldBe(TimeSpan.FromTicks(1000));
            l.Checkpoint.ShouldBeSameAs(cp);
            
            var cp2 = new Checkpoint("11", new DateTime(3500));
            var l2 = l.CreateNext(cp2);
            l2.SequentialNumber.ShouldBe(2);
            l2.Start.ShouldBe(new DateTime(2000));
            l2.End.ShouldBe(new DateTime(3500));
            l2.Duration.ShouldBe(TimeSpan.FromTicks(1500));
            l2.AggDuration.ShouldBe(TimeSpan.FromTicks(2500));
            l2.Checkpoint.ShouldBeSameAs(cp2);
        }
    }
}