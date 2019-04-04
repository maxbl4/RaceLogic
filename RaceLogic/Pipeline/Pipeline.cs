using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RaceLogic.Checkpoints;
using RaceLogic.RoundTiming;

namespace RaceLogic.Pipeline
{
    public class Pipeline<TRiderId>: IDisposable
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly IFinishCriteria finishCriteria;
        readonly CompositeDisposable disposable;
        private TrackOfCheckpoints<TRiderId> track;
        readonly Subject<List<RoundPosition<TRiderId>>> sequence = new Subject<List<RoundPosition<TRiderId>>>();
        public IObservable<List<RoundPosition<TRiderId>>> Sequence => sequence;

        public Pipeline(IEnumerable<IObservable<Checkpoint<TRiderId>>> checkpointProviders,
            IFinishCriteria finishCriteria,
            ICheckpointAggregator<TRiderId> checkpointAggregator = null)
        {
            this.finishCriteria = finishCriteria;
            disposable = new CompositeDisposable(checkpointProviders.Select(x =>
            {
                if (checkpointAggregator == null)
                    return ObservableExtensions.Subscribe<Checkpoint<TRiderId>>(x, OnCheckpoint);
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
            sequence.OnNext(track.Sequence);
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}