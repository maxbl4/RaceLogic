using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Checkpoints;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class TimestampCheckpointAggregatorTests
    {
        List<Checkpoint<int>> checkpoints = new List<Checkpoint<int>>(); 
        List<AggCheckpoint<int>> aggCheckpoints = new List<AggCheckpoint<int>>(); 
        TimestampCheckpointAggregator<int> aggregator = new TimestampCheckpointAggregator<int>(TimeSpan.FromTicks(10));
        
        public TimestampCheckpointAggregatorTests()
        {
            aggregator.Checkpoints.Subscribe(checkpoints.Add);
            aggregator.AggregatedCheckpoints.Subscribe(aggCheckpoints.Add);
        }
        
        [Fact]
        public void Aggregate_primitive_example()
        {
            var aggRecords = TimestampCheckpointAggregator<int>.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3)
            }.ToList(), TimeSpan.FromTicks(10));
            aggRecords.Count.ShouldBe(3);
            aggRecords.ShouldAllBe(x => x.Count == 1);
            aggRecords[0].RiderId.ShouldBe(1);
            aggRecords[1].RiderId.ShouldBe(2);
            aggRecords[2].RiderId.ShouldBe(3);
        }
        
        [Fact]
        public void Aggregate_two_laps_no_overlap()
        {
            var aggRecords = TimestampCheckpointAggregator<int>.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3),
                G(1,21),
                G(2,22),
                G(3,23),
            }.ToList(), TimeSpan.FromTicks(10)).ToList();
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
            var aggRecords = TimestampCheckpointAggregator<int>.AggregateOnce(new[] {
                G(1,1),
                G(2,2),
                G(3,3),
                G(1,7),
                G(2,8),
                G(3,9),
                G(1,21),
                G(2,22),
                G(3,23),
            }.ToList(), TimeSpan.FromTicks(10)).ToList();
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
        
        [Fact]
        public void Streaming_simple()
        {
            aggregator.OnNext(G(1,100));
            checkpoints.Count.ShouldBe(1);
            checkpoints[0].RiderId.ShouldBe(1);
            checkpoints[0].Timestamp.ShouldBe(new DateTime(100));
            aggCheckpoints.Count.ShouldBe(0);
            
            aggregator.OnNext(G(1,120));
            
            checkpoints.Count.ShouldBe(2);
            checkpoints[1].RiderId.ShouldBe(1);
            checkpoints[1].Timestamp.ShouldBe(new DateTime(120));
            aggCheckpoints.Count.ShouldBe(1);
            aggCheckpoints[0].RiderId.ShouldBe(1);
            aggCheckpoints[0].Timestamp.ShouldBe(new DateTime(100));
            aggCheckpoints[0].LastSeen.ShouldBe(new DateTime(100));
            
            aggregator.OnNext(G(1,125));
            aggregator.OnNext(G(1,140));
            
            checkpoints.Count.ShouldBe(3);
            checkpoints[2].RiderId.ShouldBe(1);
            checkpoints[2].Timestamp.ShouldBe(new DateTime(140));
            aggCheckpoints.Count.ShouldBe(2);
            aggCheckpoints[1].RiderId.ShouldBe(1);
            aggCheckpoints[1].Timestamp.ShouldBe(new DateTime(120));
            aggCheckpoints[1].LastSeen.ShouldBe(new DateTime(125));
        }
        
        [Fact]
        public void Streaming_should_send_all_agg_checkpoints_on_complete()
        {
            aggregator.OnNext(G(1,100));
            aggregator.OnNext(G(1,101));
            aggregator.OnNext(G(1,102));
            checkpoints.Count.ShouldBe(1);
            checkpoints[0].RiderId.ShouldBe(1);
            checkpoints[0].Timestamp.ShouldBe(new DateTime(100));
            aggCheckpoints.Count.ShouldBe(0);
            
            aggregator.OnCompleted();
            aggCheckpoints.Count.ShouldBe(1);
            aggCheckpoints[0].Timestamp.ShouldBe(new DateTime(100));
            aggCheckpoints[0].LastSeen.ShouldBe(new DateTime(102));
        }

        Checkpoint<int> G(int rider, int timeOffset)
        {
            return new Checkpoint<int>(rider, new DateTime(timeOffset));
        }
    }
}