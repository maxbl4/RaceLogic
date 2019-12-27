using System;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Model
{
    public class CheckpointTests
    {
        [Fact]
        public void Checkpoint_should_initialize_correctly()
        {
            var ts = new DateTime(1234567);
            var cp = new Checkpoint("123", ts);
            cp.RiderId.Should().Be("123");
            cp.Timestamp.Should().Be(ts);
            
            cp = new Checkpoint("456");
            cp.RiderId.Should().Be("456");
            cp.Timestamp.Should().Be(default);
        }
        
        [Fact]
        public void AggCheckpoint_add_should_work()
        {
            var agg = AggCheckpoint.From(new Checkpoint("11"));
            agg.Count.Should().Be(1);
            agg.Timestamp.Should().Be(default);
            agg.LastSeen.Should().Be(default);

            var ts = new DateTime(1234567);
            var agg2 = agg.Add(new Checkpoint("11", ts));
            
            agg.Count.Should().Be(1);
            agg.Timestamp.Should().Be(default);
            agg.LastSeen.Should().Be(default);
            
            agg2.Count.Should().Be(2);
            agg2.Timestamp.Should().Be(ts);
            agg2.LastSeen.Should().Be(ts);

            Assert.Throws<ArgumentException>(() => agg.Add(new Checkpoint("12")));
        }
        
        [Fact]
        public void AggCheckpoint_rps_via_add()
        {
            var ts = new DateTime(10000000, DateTimeKind.Utc);
            var agg = AggCheckpoint.From(new Checkpoint("11", ts));
            agg.Timestamp.Should().Be(ts);
            agg.LastSeen.Should().Be(ts);
            agg.Rps.Should().Be(1);

            var ts2 = ts.AddSeconds(0.5);
            agg = agg.Add(new Checkpoint("11", ts2));
            agg.Timestamp.Should().Be(ts);
            agg.LastSeen.Should().Be(ts2);
            agg.Count.Should().Be(2);
            agg.Rps.Should().Be(4);
            
            var ts3 = ts.AddSeconds(3);
            agg = agg.Add(new Checkpoint("11", ts3));
            agg.Timestamp.Should().Be(ts);
            agg.LastSeen.Should().Be(ts3);
            agg.Count.Should().Be(3);
            agg.Rps.Should().Be(1);
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
            agg.Timestamp.Should().Be(ts);
            agg.LastSeen.Should().Be(ts.AddSeconds(2));
            agg.Count.Should().Be(3);
            agg.Rps.Should().Be(2);
        }
        
        [Fact]
        public void AggCheckpoint_rps_same_timestamps()
        {
            var ts = new DateTime(10000000, DateTimeKind.Utc);
            var agg = new AggCheckpoint("11", ts, ts, 2, false);
            agg.Rps.Should().Be(2);

            agg = AggCheckpoint.From(new []
            {
                new Checkpoint("11", ts),
                new Checkpoint("11", ts),
                new Checkpoint("11", ts),
            });
            agg.Timestamp.Should().Be(ts);
            agg.LastSeen.Should().Be(ts);
            agg.Count.Should().Be(3);
            agg.Rps.Should().Be(3);
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
            agg.Count.Should().Be(5);
            agg.Timestamp.Should().Be(new DateTime(1000));
            agg.LastSeen.Should().Be(new DateTime(1003));
            
            var agg2 = agg.Add(new Checkpoint("11", new DateTime(1004)));
            
            agg2.Count.Should().Be(6);
            agg2.Timestamp.Should().Be(new DateTime(1000));
            agg2.LastSeen.Should().Be(new DateTime(1004));
            
            agg2 = agg2.Add(new Checkpoint("11", new DateTime(999)));
            
            agg2.Count.Should().Be(7);
            agg2.Timestamp.Should().Be(new DateTime(999));
            agg2.LastSeen.Should().Be(new DateTime(1004));
            
            agg.Count.Should().Be(5);
            agg.Timestamp.Should().Be(new DateTime(1000));
            agg.LastSeen.Should().Be(new DateTime(1003));
            
            agg = AggCheckpoint.From(new Checkpoint[0]);
            agg.Count.Should().Be(0);
            agg.Timestamp.Should().Be(default);
        }
    }
}