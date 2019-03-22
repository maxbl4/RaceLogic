using System;
using RaceLogic.Checkpoints;
using RaceLogic.Model;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Model
{
    public class LapTests
    {
        [Fact]
        public void Laps_without_timestamps()
        {
            var cp = new Checkpoint<int>(11);
            var l = new Lap<int>(cp, new DateTime(1000));
            l.SequentialNumber.ShouldBe(1);
            l.Start.ShouldBe(default(DateTime));
            l.End.ShouldBe(default(DateTime));
            l.Duration.ShouldBe(TimeSpan.Zero);
            l.AggDuration.ShouldBe(TimeSpan.Zero);
            l.Checkpoint.ShouldBeSameAs(cp);
            
            var cp2 = new Checkpoint<int>(11);
            var l2 = l.CreateNext(cp2);
            l2.SequentialNumber.ShouldBe(2);
            l2.Start.ShouldBe(default(DateTime));
            l2.End.ShouldBe(default(DateTime));
            l2.Duration.ShouldBe(TimeSpan.Zero);
            l2.AggDuration.ShouldBe(TimeSpan.Zero);
            l2.Checkpoint.ShouldBeSameAs(cp2);
        }
        
        [Fact]
        public void Laps_with_timestamps()
        {
            var cp = new Checkpoint<int>(11, new DateTime(2000));
            var l = new Lap<int>(cp, new DateTime(1000));
            l.SequentialNumber.ShouldBe(1);
            l.Start.ShouldBe(new DateTime(1000));
            l.End.ShouldBe(new DateTime(2000));
            l.Duration.ShouldBe(TimeSpan.FromTicks(1000));
            l.AggDuration.ShouldBe(TimeSpan.FromTicks(1000));
            l.Checkpoint.ShouldBeSameAs(cp);
            
            var cp2 = new Checkpoint<int>(11, new DateTime(3500));
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