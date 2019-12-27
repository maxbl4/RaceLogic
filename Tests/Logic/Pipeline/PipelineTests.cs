using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.Pipeline;
using maxbl4.Race.Logic.RiderIdResolving;
using maxbl4.Race.Logic.RoundTiming;
using Xunit;

namespace maxbl4.Race.Tests.Logic.Pipeline
{
    public class PipelineTests
    {
        [Fact]
        public async Task Should_be_able_to_setup_pipeline()
        {
            var roundDuration = TimeSpan.FromTicks(30);
            var minLap = TimeSpan.FromTicks(5);
            var roundStartTime = new DateTime(1000);
            List<RoundPosition> sequence = null;
            
            var riderIdMap = new Dictionary<string, string>{{"11", "Eleven"}, {"12", "Twelve"}, {"13", "Thirteen"}};
            var riderIdResolver = new SimpleMapRiderIdResolver(riderIdMap, 
                input => Task.FromResult(riderIdMap[input] = input.ToString()));
            var manualCheckpointProvider = new CheckpointProvider(riderIdResolver);
            var builder = new PipelineBuilder()
                .WithFinishCriteria(FinishCriteria.FromDuration(roundDuration))
                .WithCheckpointAggregator(new TimestampCheckpointAggregator(minLap))
                .WithCheckpointProvider(manualCheckpointProvider);
            var pp = builder.Build();
            pp.Sequence.Subscribe(x => sequence = x);
            
            pp.StartRound(roundStartTime);
            await manualCheckpointProvider.ProvideInput("11", new DateTime(1001));
            await manualCheckpointProvider.ProvideInput("15", new DateTime(1005));
            sequence.Should().NotBeNull();
            sequence.Count.Should().Be(2);
            sequence[0].RiderId.Should().Be("Eleven");
            sequence[1].RiderId.Should().Be("15");
        }

        [Fact]
        public async Task Should()
        {
            var rfidToRider = new ConcurrentDictionary<string, string>
            {
                ["ABC"] = "Rider_1",
                ["XXX"] = "Rider_2",
            };
            var rfidResolver = new SimpleMapRiderIdResolver(rfidToRider, x => Task.FromResult($"Rider_{x}"));
            var rfidCps = new List<Checkpoint>{new Checkpoint("ABC"), new Checkpoint("XXX")};
            var resolvedCps = await rfidResolver.ResolveAll(rfidCps).ToListAsync();
            resolvedCps.Should().HaveCount(2);
        }
    }
}