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
            cp.Timestamp.ShouldBe(default(DateTime));
        }
        
        [Fact]
        public void AggCheckpoint_add_should_work()
        {
            var agg = AggCheckpoint.From(new Checkpoint("11"));
            agg.Histogram.ShouldBe("Checkpoint = 1");
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default);
            agg.LastSeen.ShouldBe(default);

            var ts = new DateTime(1234567);
            var agg2 = agg.Add(new RfidCheckpoint("11", ts, "123"));
            
            agg.Histogram.ShouldBe("Checkpoint = 1");
            agg.Count.ShouldBe(1);
            agg.Timestamp.ShouldBe(default);
            agg.LastSeen.ShouldBe(default);
            
            agg2.Histogram.ShouldBe("Checkpoint = 1, RfidCheckpoint = 1");
            agg2.Count.ShouldBe(2);
            agg2.Timestamp.ShouldBe(ts);
            agg2.LastSeen.ShouldBe(ts);

            Assert.Throws<ArgumentException>(() => agg.Add(new Checkpoint("12")));
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
                new RfidCheckpoint("11", new DateTime(1001), "tag1"), 
                new RfidCheckpoint("11", new DateTime(1003), "tag1"),
            });
            agg.Histogram.ShouldBe("Checkpoint = 3, RfidCheckpoint = 2");
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            var agg2 = agg.Add(new RfidCheckpoint("11", new DateTime(1004), "123"));
            
            agg2.Histogram.ShouldBe("Checkpoint = 3, RfidCheckpoint = 3");
            agg2.Count.ShouldBe(6);
            agg2.Timestamp.ShouldBe(new DateTime(1000));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg2 = agg2.Add(new Checkpoint("11", new DateTime(999)));
            
            agg2.Histogram.ShouldBe("Checkpoint = 4, RfidCheckpoint = 3");
            agg2.Count.ShouldBe(7);
            agg2.Timestamp.ShouldBe(new DateTime(999));
            agg2.LastSeen.ShouldBe(new DateTime(1004));
            
            agg.Histogram.ShouldBe("Checkpoint = 3, RfidCheckpoint = 2");
            agg.Count.ShouldBe(5);
            agg.Timestamp.ShouldBe(new DateTime(1000));
            agg.LastSeen.ShouldBe(new DateTime(1003));
            
            agg = AggCheckpoint.From(new Checkpoint[0]);
            agg.Count.ShouldBe(0);
            agg.Histogram.ShouldBeEmpty();
            agg.Timestamp.ShouldBe(default);
        }
    }
}