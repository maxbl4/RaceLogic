using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using Xunit;

namespace maxbl4.Race.Tests.Logic
{
    public class TimestampCheckpointAggregatorTests
    {
        private readonly List<Checkpoint> aggCheckpoints = new();

        private readonly TimestampAggregator<Checkpoint> aggregator =
            TimestampAggregatorConfigurations.ForCheckpoint(TimeSpan.FromTicks(10));

        private readonly List<Checkpoint> checkpoints = new();

        public TimestampCheckpointAggregatorTests()
        {
            aggregator.Checkpoints.Subscribe(checkpoints.Add);
            aggregator.AggregatedCheckpoints.Subscribe(aggCheckpoints.Add);
        }

        [Fact]
        public void Aggregate_primitive_example()
        {
            var aggRecords = aggregator.AggregateOnce(new[]
            {
                G("1", 1),
                G("2", 2),
                G("3", 3)
            }.ToList(), TimeSpan.FromTicks(10), Checkpoint.TimestampComparer);
            aggRecords.Count.Should().Be(3);
            aggRecords.Should().OnlyContain(x => x.Count == 1);
            aggRecords[0].RiderId.Should().Be("1");
            aggRecords[1].RiderId.Should().Be("2");
            aggRecords[2].RiderId.Should().Be("3");
        }

        [Fact]
        public void Aggregate_two_laps_no_overlap()
        {
            var aggRecords = aggregator.AggregateOnce(new[]
            {
                G("1", 1),
                G("2", 2),
                G("3", 3),
                G("1", 21),
                G("2", 22),
                G("3", 23)
            }.ToList(), TimeSpan.FromTicks(10), Checkpoint.TimestampComparer).ToList();
            aggRecords.Count.Should().Be(6);
            aggRecords.Should().OnlyContain(x => x.Count == 1);
            aggRecords[0].RiderId.Should().Be("1");
            aggRecords[1].RiderId.Should().Be("2");
            aggRecords[2].RiderId.Should().Be("3");
            aggRecords[3].RiderId.Should().Be("1");
            aggRecords[4].RiderId.Should().Be("2");
            aggRecords[5].RiderId.Should().Be("3");
        }

        [Fact]
        public void Aggregate_two_laps_with_one_overlap()
        {
            var aggRecords = aggregator.AggregateOnce(new[]
            {
                G("1", 1),
                G("2", 2),
                G("3", 3),
                G("1", 7),
                G("2", 8),
                G("3", 9),
                G("1", 21),
                G("2", 22),
                G("3", 23)
            }.ToList(), TimeSpan.FromTicks(10), Checkpoint.TimestampComparer).ToList();
            aggRecords.Count.Should().Be(6);
            aggRecords.Take(3).Should().OnlyContain(x => x.Count == 2);
            aggRecords.Skip(3).Should().OnlyContain(x => x.Count == 1);
            aggRecords[0].RiderId.Should().Be("1");
            aggRecords[1].RiderId.Should().Be("2");
            aggRecords[2].RiderId.Should().Be("3");
            aggRecords[3].RiderId.Should().Be("1");
            aggRecords[4].RiderId.Should().Be("2");
            aggRecords[5].RiderId.Should().Be("3");
        }

        [Fact]
        public void Streaming_simple()
        {
            aggregator.OnNext(G("1", 100));
            checkpoints.Count.Should().Be(1);
            checkpoints[0].RiderId.Should().Be("1");
            checkpoints[0].Timestamp.Should().Be(new DateTime(100));
            aggCheckpoints.Count.Should().Be(0);

            aggregator.OnNext(G("1", 120));

            checkpoints.Count.Should().Be(2);
            checkpoints[1].RiderId.Should().Be("1");
            checkpoints[1].Timestamp.Should().Be(new DateTime(120));
            aggCheckpoints.Count.Should().Be(1);
            aggCheckpoints[0].RiderId.Should().Be("1");
            aggCheckpoints[0].Timestamp.Should().Be(new DateTime(100));
            aggCheckpoints[0].LastSeen.Should().Be(new DateTime(100));

            aggregator.OnNext(G("1", 125));
            aggregator.OnNext(G("1", 140));

            checkpoints.Count.Should().Be(3);
            checkpoints[2].RiderId.Should().Be("1");
            checkpoints[2].Timestamp.Should().Be(new DateTime(140));
            aggCheckpoints.Count.Should().Be(2);
            aggCheckpoints[1].RiderId.Should().Be("1");
            aggCheckpoints[1].Timestamp.Should().Be(new DateTime(120));
            aggCheckpoints[1].LastSeen.Should().Be(new DateTime(125));
        }

        [Fact]
        public void Streaming_should_send_all_agg_checkpoints_on_complete()
        {
            aggregator.OnNext(G("1", 100));
            aggregator.OnNext(G("1", 101));
            aggregator.OnNext(G("1", 102));
            checkpoints.Count.Should().Be(1);
            checkpoints[0].RiderId.Should().Be("1");
            checkpoints[0].Timestamp.Should().Be(new DateTime(100));
            aggCheckpoints.Count.Should().Be(0);

            aggregator.OnCompleted();
            aggCheckpoints.Count.Should().Be(1);
            aggCheckpoints[0].Timestamp.Should().Be(new DateTime(100));
            aggCheckpoints[0].LastSeen.Should().Be(new DateTime(102));
        }

        private Checkpoint G(string rider, int timeOffset)
        {
            return new(rider, new DateTime(timeOffset));
        }
    }
}