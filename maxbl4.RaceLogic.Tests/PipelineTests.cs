using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Pipeline;
using maxbl4.RaceLogic.RiderIdResolving;
using maxbl4.RaceLogic.RoundTiming;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests
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
            sequence.ShouldNotBeNull();
            sequence.Count.ShouldBe(2);
            sequence[0].RiderId.ShouldBe("Eleven");
            sequence[1].RiderId.ShouldBe("15");
        }
    }
}