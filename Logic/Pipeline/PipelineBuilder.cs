using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.Pipeline
{
    public class PipelineBuilder
    {
        private List<IObservable<Checkpoint>> checkpointProviders = new List<IObservable<Checkpoint>>();
        ICheckpointAggregator checkpointAggregator;
        private IFinishCriteria finishCriteria;

        public PipelineBuilder WithCheckpointAggregator(ICheckpointAggregator aggregator)
        {
            checkpointAggregator = aggregator;
            return this;
        }

        public PipelineBuilder WithFinishCriteria(IFinishCriteria criteria)
        {
            finishCriteria = criteria;
            return this;
        }

        public PipelineBuilder WithCheckpointProvider(params IObservable<Checkpoint>[] checkpointProvider)
        {
            checkpointProviders = checkpointProvider.ToList();
            return this;
        }

        public Pipeline Build()
        {
            return new Pipeline(checkpointProviders, finishCriteria, checkpointAggregator);
        }
    }
}