using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.RoundTiming;
using maxbl4.RaceLogic.Extensions;

namespace maxbl4.RaceLogic.Pipeline
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

        public PipelineBuilder WithCheckpointProvider(IObservable<Checkpoint> checkpointProvider)
        {
            checkpointProviders.Add(checkpointProvider);
            return this;
        }

        public Pipeline Build()
        {
            return new Pipeline(checkpointProviders, finishCriteria, checkpointAggregator);
        }
    }
}