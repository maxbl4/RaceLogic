using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RaceLogic.Checkpoints;
using RaceLogic.Extensions;
using RaceLogic.Model;

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

    public class Pipeline<TRiderId>: IDisposable
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly IFinishCriteria finishCriteria;
        readonly CompositeDisposable disposable;
        private TrackOfCheckpoints<TRiderId> track;

        public Pipeline(IEnumerable<IObservable<Checkpoint<TRiderId>>> checkpointProviders,
            IFinishCriteria finishCriteria,
            ICheckpointAggregator<TRiderId> checkpointAggregator = null)
        {
            this.finishCriteria = finishCriteria;
            disposable = new CompositeDisposable(checkpointProviders.Select(x =>
            {
                if (checkpointAggregator == null)
                    return x.Subscribe(OnCheckpoint);
                return x.Subscribe(checkpointAggregator);
            }));
            if (checkpointAggregator != null)
                disposable.Add(checkpointAggregator.Subscribe(OnCheckpoint));
        }

        public void StartRound(DateTime roundStartTime)
        {
            track = new TrackOfCheckpoints<TRiderId>(roundStartTime, finishCriteria);
        }

        public void StopRound()
        {
            track?.ForceFinish();
        }

        void OnCheckpoint(Checkpoint<TRiderId> cp)
        {
            if (track == null)
                StartRound(cp.Timestamp);
            track.Append(cp);
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}