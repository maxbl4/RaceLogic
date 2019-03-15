using System;
using System.Linq;
using RaceLogic.Model;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class TimestampCheckpointAggregatorTests
    {
        [Fact]
        public void Aggregate_primitive_example()
        {
            var aggRecords = TimestampCheckpointAggregator.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3)
            }.ToList(), TimeSpan.FromSeconds(10));
            aggRecords.Count.ShouldBe(3);
            aggRecords.ShouldAllBe(x => x.Count == 1);
            aggRecords[0].RiderId.ShouldBe(1);
            aggRecords[1].RiderId.ShouldBe(2);
            aggRecords[2].RiderId.ShouldBe(3);
        }
        
        [Fact]
        public void Aggregate_two_laps_no_overlap()
        {
            var aggRecords = TimestampCheckpointAggregator.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3),
                G(1,21),
                G(2,22),
                G(3,23),
            }.ToList(), TimeSpan.FromSeconds(10)).ToList();
            aggRecords.Count.ShouldBe(6);
            aggRecords.ShouldAllBe(x => x.Count == 1);
            aggRecords[0].RiderId.ShouldBe(1);
            aggRecords[1].RiderId.ShouldBe(2);
            aggRecords[2].RiderId.ShouldBe(3);
            aggRecords[3].RiderId.ShouldBe(1);
            aggRecords[4].RiderId.ShouldBe(2);
            aggRecords[5].RiderId.ShouldBe(3);
        }
        
        [Fact]
        public void Aggregate_two_laps_with_one_overlap()
        {
            var aggRecords = TimestampCheckpointAggregator.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3),
                G(1,7),
                G(2,8),
                G(3,9),
                G(1,21),
                G(2,22),
                G(3,23),
            }.ToList(), TimeSpan.FromSeconds(10)).ToList();
            aggRecords.Count.ShouldBe(6);
            aggRecords.Take(3).ShouldAllBe(x => x.Count == 2);
            aggRecords.Skip(3).ShouldAllBe(x => x.Count == 1);
            aggRecords[0].RiderId.ShouldBe(1);
            aggRecords[1].RiderId.ShouldBe(2);
            aggRecords[2].RiderId.ShouldBe(3);
            aggRecords[3].RiderId.ShouldBe(1);
            aggRecords[4].RiderId.ShouldBe(2);
            aggRecords[5].RiderId.ShouldBe(3);
        }

        Checkpoint<int> G(int rider, int timeOffset)
        {
            return new Checkpoint<int>(rider, Offset(timeOffset));
        }

        DateTime Offset(int timeOffset)
        {
            return new DateTime(2018, 1, 1, 1, 1, timeOffset);
        }
    }
}