using System;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;
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
            l.SequentialNumber.Should().Be(1);
            l.Start.Should().Be(new DateTime(1000));
            l.End.Should().Be(new DateTime(2000));
            l.Duration.Should().Be(TimeSpan.FromTicks(1000));
            l.AggDuration.Should().Be(TimeSpan.FromTicks(1000));
            l.Checkpoint.Should().BeSameAs(cp);
            
            var cp2 = new Checkpoint("11", new DateTime(3500));
            var l2 = l.CreateNext(cp2);
            l2.SequentialNumber.Should().Be(2);
            l2.Start.Should().Be(new DateTime(2000));
            l2.End.Should().Be(new DateTime(3500));
            l2.Duration.Should().Be(TimeSpan.FromTicks(1500));
            l2.AggDuration.Should().Be(TimeSpan.FromTicks(2500));
            l2.Checkpoint.Should().BeSameAs(cp2);
        }
    }
}