using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RaceLogic.Checkpoints;
using RaceLogic.Extensions;
using RaceLogic.RoundTiming;

namespace RaceLogic.Pipeline
{
    public class PipelineBuilder<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        private List<IObservable<Checkpoint<TRiderId>>> checkpointProviders = new List<IObservable<Checkpoint<TRiderId>>>();
        ICheckpointAggregator<TRiderId> checkpointAggregator;
        private IFinishCriteria finishCriteria;

        public PipelineBuilder<TRiderId> WithCheckpointAggregator(ICheckpointAggregator<TRiderId> aggregator)
        {
            checkpointAggregator = aggregator;
            return this;
        }

        public PipelineBuilder<TRiderId> WithFinishCriteria(IFinishCriteria criteria)
        {
            finishCriteria = criteria;
            return this;
        }

        public PipelineBuilder<TRiderId> WithCheckpointProvider(IObservable<Checkpoint<TRiderId>> checkpointProvider)
        {
            checkpointProviders.Add(checkpointProvider);
            return this;
        }

        public Pipeline<TRiderId> Build()
        {
            return new Pipeline<TRiderId>(checkpointProviders, finishCriteria, checkpointAggregator);
        }
    }
}