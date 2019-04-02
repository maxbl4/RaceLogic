using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaceLogic.Checkpoints;
using RaceLogic.Model;
using RaceLogic.Pipeline;
using RaceLogic.ReferenceModel;
using RaceLogic.RiderIdResolving;
using Xunit;

namespace RaceLogic.Tests
{
    public class PipelineTests
    {
        [Fact]
        public void Should_be_able_to_setup_pipeline()
        {
            var roundDuration = TimeSpan.FromSeconds(30);
            var minLap = TimeSpan.FromSeconds(5);
            var roundStartTime = new DateTime(1000);
            
            var riderIdMap = new Dictionary<int, string>();
            var riderIdResolver = new SimpleMapRiderIdResolver<int, string>(riderIdMap, 
                input => Task.FromResult(riderIdMap[input] = input.ToString()));
            var manualCheckpointProvider = new CheckpointProvider<int, string>(riderIdResolver);
            var builder = new PipelineBuilder<string>()
                .WithFinishCriteria(FinishCriteria.FromDuration(roundDuration))
                .WithCheckpointAggregator(new TimestampCheckpointAggregator<string>(minLap))
                .WithCheckpointProvider(manualCheckpointProvider);
            var pp = builder.Build();
            
            pp.StartRound(roundStartTime);
        }
    }
}