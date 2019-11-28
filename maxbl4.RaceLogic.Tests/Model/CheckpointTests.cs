using System;
using maxbl4.RaceLogic.Checkpoints;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.Model
{
    public class CheckpointTests
    {
        [Fact]
        public void Checkpoint_should_initialize_correctly()
        {
            var ts = new DateTime(1234567);
            var cp = new Checkpoint("123", ts);
            cp.RiderId.ShouldBe("123");
            cp.Timestamp.ShouldBe(ts);
            
            cp = new Checkpoint("456");
            cp.RiderId.ShouldBe("456");
            cp.Timestamp.ShouldBe(default);
        }
        
        [Fact]
        public void AggCheckpoint_add_should_work()
        {
            var agg = AggCheckpoint.From(new Checkpoint("11"));
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default);
            agg.LastSeen.ShouldBe(default);

            var ts = new DateTime(1234567);
            var agg2 = agg.Add(new Checkpoint("11", ts));
            
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default);
            agg.LastSeen.ShouldBe(default);
            
            agg2.Count.ShouldBe(2);
            agg2.Timestamp.ShouldBe(ts);
            agg2.LastSeen.ShouldBe(ts);

            Assert.Throws<ArgumentException>(() => agg.Add(new Checkpoint("12")));
        }
        
        [Fact]
        public void AggCheckpoint_rps_via_add()
        {
            var ts = new DateTime(10000000, DateTimeKind.Utc);
            var agg = AggCheckpoint.From(new Checkpoint("11", ts));
            agg.Timestamp.ShouldBe(ts);
            agg.LastSeen.ShouldBe(ts);
            agg.Rps.ShouldBe(1);

            var ts2 = ts.AddSeconds(0.5);
            agg = agg.Add(new Checkpoint("11", ts2));
            agg.Timestamp.ShouldBe(ts);
            agg.LastSeen.ShouldBe(ts2);
            agg.Count.ShouldBe(2);
            agg.Rps.ShouldBe(4);
            
            var ts3 = ts.AddSeconds(3);
            agg = agg.Add(new Checkpoint("11", ts3));
            agg.Timestamp.ShouldBe(ts);
            agg.LastSeen.ShouldBe(ts3);
            agg.Count.ShouldBe(3);
            agg.Rps.ShouldBe(1);
        }
        
        [Fact]
        public void AggCheckpoint_rps_via_from()
        {
            var ts = new DateTime(10000000, DateTimeKind.Utc);
            var agg = AggCheckpoint.From(new []
            {
                new Checkpoint("11", ts),
                new Checkpoint("11", ts.AddSeconds(1)),
                new Checkpoint("11", ts.AddSeconds(2)),
            });
            agg.Timestamp.ShouldBe(ts);
            agg.LastSeen.ShouldBe(ts.AddSeconds(2));
            agg.Count.ShouldBe(3);
            agg.Rps.ShouldBe(2);
        }
        
        [Fact]
        public void AggCheckpoint_rps_same_timestamps()
        {
            var ts = new DateTime(10000000, DateTimeKind.Utc);
            var agg = new AggCheckpoint("11", ts, ts, 2, false);
            agg.Rps.ShouldBe(2);

            agg = AggCheckpoint.From(new []
            {
                new Checkpoint("11", ts),
                new Checkpoint("11", ts),
                new Checkpoint("11", ts),
            });
            agg.Timestamp.ShouldBe(ts);
            agg.LastSeen.ShouldBe(ts);
            agg.Count.ShouldBe(3);
            agg.Rps.ShouldBe(3);
        }
        
        [Fact]
        public void AggCheckpoint_from_many_should_work()
        {
            Assert.Throws<ArgumentException>(() => AggCheckpoint.From(new[]
            {
                new Checkpoint("11"),
                new Checkpoint("12"),
            }));
            
            var agg = AggCheckpoint.From(new []
            {
                new Checkpoint("11", new DateTime(1000)),
                new Checkpoint("11", new DateTime(1001)),
                new Checkpoint("11", new DateTime(1002)),
                new Checkpoint("11", new DateTime(1001)), 
                new Checkpoint("11", new DateTime(1003)),
            });
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            var agg2 = agg.Add(new Checkpoint("11", new DateTime(1004)));
            
            agg2.Count.ShouldBe(6);
            agg2.Timestamp.ShouldBe(new DateTime(1000));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg2 = agg2.Add(new Checkpoint("11", new DateTime(999)));
            
            agg2.Count.ShouldBe(7);
            agg2.Timestamp.ShouldBe(new DateTime(999));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            agg = AggCheckpoint.From(new Checkpoint[0]);
            agg.Count.ShouldBe(0);
            agg.Timestamp.ShouldBe(default);
        }
    }
}