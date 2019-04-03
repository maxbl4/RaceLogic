using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaceLogic.Checkpoints;
using RaceLogic.Model;
using RaceLogic.Pipeline;
using RaceLogic.ReferenceModel;
using RaceLogic.RiderIdResolving;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class PipelineTests
    {
        [Fact]
        public async Task Should_be_able_to_setup_pipeline()
        {
            var roundDuration = TimeSpan.FromTicks(30);
            var minLap = TimeSpan.FromTicks(5);
            var roundStartTime = new DateTime(1000);
            List<RoundPosition<string>> sequence = null;
            
            var riderIdMap = new Dictionary<int, string>{{11, "Eleven"}, {12, "Twelve"}, {13, "Thirteen"}};
            var riderIdResolver = new SimpleMapRiderIdResolver<int, string>(riderIdMap, 
                input => Task.FromResult(riderIdMap[input] = input.ToString()));
            var manualCheckpointProvider = new CheckpointProvider<int, string>(riderIdResolver);
            var builder = new PipelineBuilder<string>()
                .WithFinishCriteria(FinishCriteria.FromDuration(roundDuration))
                .WithCheckpointAggregator(new TimestampCheckpointAggregator<string>(minLap))
                .WithCheckpointProvider(manualCheckpointProvider);
            var pp = builder.Build();
            pp.Sequence.Subscribe(x => sequence = x);
            
            pp.StartRound(roundStartTime);
            await manualCheckpointProvider.ProvideInput(11, new DateTime(1001));
            await manualCheckpointProvider.ProvideInput(15, new DateTime(1005));
            sequence.ShouldNotBeNull();
            sequence.Count.ShouldBe(2);
            sequence[0].RiderId.ShouldBe("Eleven");
            sequence[1].RiderId.ShouldBe("15");
        }
    }
}