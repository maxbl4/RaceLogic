using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Model
{
    public class RoundPositionTests
    {
        [Fact]
        public void Should_compare_unfinished()
        {
            var p1 = RoundPosition.FromLaps("11", MakeLaps("11", 3), false);
            var p2 = RoundPosition.FromLaps("12", MakeLaps("11", 4), false);
            RoundPosition.LapsCountFinishedComparer.Compare(p2, p1).Should().BeNegative();
        }

        [Fact]
        public void Should_compare_finished()
        {
            var p1 = RoundPosition.FromLaps("11", MakeLaps("11", 3), true);
            var p2 = RoundPosition.FromLaps("12", MakeLaps("11", 4), true);
            RoundPosition.LapsCountFinishedComparer.Compare(p2, p1).Should().BeNegative();
        }

        [Fact]
        public void Should_compare_finished_should_be_smaller()
        {
            var p1 = RoundPosition.FromLaps("11", MakeLaps("11", 1), false);
            var p2 = RoundPosition.FromLaps("12", MakeLaps("11", 1), true);
            RoundPosition.LapsCountFinishedComparer.Compare(p2, p1).Should().BeNegative();
        }

        [Fact]
        public void Should_compare_finished_should_be_smaller_with_less_laps()
        {
            var p1 = RoundPosition.FromLaps("11", MakeLaps("11", 5), false);
            var p2 = RoundPosition.FromLaps("12", MakeLaps("11", 1), true);
            RoundPosition.LapsCountFinishedComparer.Compare(p2, p1).Should().BeNegative();
        }

        [Fact]
        public void Should_compare_simple_types()
        {
            1.CompareTo(2).Should().BeNegative();
            false.CompareTo(true).Should().BeNegative();
        }

        [Fact]
        public void Should_make_laps()
        {
            var laps = MakeLaps("11", 3).ToList();
            laps.Should().HaveCount(3).And.SatisfyRespectively(
                x => x.SequentialNumber.Should().Be(1),
                x => x.SequentialNumber.Should().Be(2),
                x => x.SequentialNumber.Should().Be(3)
            );
        }

        private IEnumerable<Lap> MakeLaps(string riderId, int count)
        {
            Lap lap = null;
            for (var i = 0; i < count; i++)
                if (i == 0)
                {
                    lap = new Lap(new Checkpoint(riderId, new DateTime(1000 + i * 100)), new DateTime(1000));
                    yield return lap;
                }
                else
                {
                    lap = lap.CreateNext(new Checkpoint(riderId, new DateTime(1000 + i * 100)));
                    yield return lap;
                }
        }
    }
}