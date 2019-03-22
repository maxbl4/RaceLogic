using System;
using RaceLogic.Checkpoints;
using RaceLogic.Checkpoints.Mutations;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Model
{
    public class CheckpointTests
    {
        [Fact]
        public void Checkpoint_should_initialize_correctly()
        {
            var ts = new DateTime(1234567);
            var cp = new Checkpoint<int>(123, ts);
            cp.RiderId.ShouldBe(123);
            cp.Timestamp.ShouldBe(ts);
            cp.HasTimestamp.ShouldBeTrue();
            
            cp = new Checkpoint<int>(456);
            cp.RiderId.ShouldBe(456);
            cp.Timestamp.ShouldBe(default(DateTime));
            cp.HasTimestamp.ShouldBeFalse();
        }
        
        [Fact]
        public void Checkpoint_should_mutate()
        {
            var ts = new DateTime(1234567);
            var cp = new Checkpoint<int>(123, ts);
            var cp2 = cp.WithRiderId(222);
            cp.RiderId.ShouldBe(123);
            cp.Timestamp.ShouldBe(ts);
            
            cp2.RiderId.ShouldBe(222);
            cp2.Timestamp.ShouldBe(ts);

            cp2 = cp.WithTimestamp(ts + TimeSpan.FromDays(1));
            cp2.RiderId.ShouldBe(123);
            cp2.Timestamp.ShouldBe(ts + TimeSpan.FromDays(1));
        }

        [Fact]
        public void AggCheckpoint_add_should_work()
        {
            var agg = AggCheckpoint<int>.From(new Checkpoint<int>(11));
            agg.Histogram.ShouldBe("Checkpoint`1 = 1");
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default(DateTime));
            agg.LastSeen.ShouldBe(default(DateTime));

            var ts = new DateTime(1234567);
            var agg2 = agg.Add(new RfidCheckpoint<int>(11, ts, "123"));
            
            agg.Histogram.ShouldBe("Checkpoint`1 = 1");
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default(DateTime));
            agg.LastSeen.ShouldBe(default(DateTime));
            
            agg2.Histogram.ShouldBe("Checkpoint`1 = 1, RfidCheckpoint`1 = 1");
            agg2.Count.ShouldBe(2);
            agg2.Timestamp.ShouldBe(ts);
            agg2.LastSeen.ShouldBe(ts);

            Assert.Throws<ArgumentException>(() => agg.Add(new Checkpoint<int>(12)));
        }
        
        [Fact]
        public void AggCheckpoint_from_many_should_work()
        {
            Assert.Throws<ArgumentException>(() => AggCheckpoint<int>.From(new[]
            {
                new Checkpoint<int>(11),
                new Checkpoint<int>(12),
            }));
            
            var agg = AggCheckpoint<int>.From(new []
            {
                new Checkpoint<int>(11, new DateTime(1000)),
                new Checkpoint<int>(11, new DateTime(1001)),
                new Checkpoint<int>(11, new DateTime(1002)),
                new RfidCheckpoint<int>(11, new DateTime(1001), "tag1"), 
                new RfidCheckpoint<int>(11, new DateTime(1003), "tag1"),
            });
            agg.Histogram.ShouldBe("Checkpoint`1 = 3, RfidCheckpoint`1 = 2");
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            var agg2 = agg.Add(new RfidCheckpoint<int>(11, new DateTime(1004), "123"));
            
            agg2.Histogram.ShouldBe("Checkpoint`1 = 3, RfidCheckpoint`1 = 3");
            agg2.Count.ShouldBe(6);
            agg2.Timestamp.ShouldBe(new DateTime(1000));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg2 = agg2.Add(new Checkpoint<int>(11, new DateTime(999)));
            
            agg2.Histogram.ShouldBe("Checkpoint`1 = 4, RfidCheckpoint`1 = 3");
            agg2.Count.ShouldBe(7);
            agg2.Timestamp.ShouldBe(new DateTime(999));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg.Histogram.ShouldBe("Checkpoint`1 = 3, RfidCheckpoint`1 = 2");
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            agg = AggCheckpoint<int>.From(new Checkpoint<int>[0]);
            agg.Count.ShouldBe(0);
            agg.Histogram.ShouldBeEmpty();
            agg.Timestamp.ShouldBe(default(DateTime));
        }
    }
}