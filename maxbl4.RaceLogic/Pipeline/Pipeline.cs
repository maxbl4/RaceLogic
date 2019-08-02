using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.RoundTiming;

namespace maxbl4.RaceLogic.Pipeline
{
    public class Pipeline: IDisposable
    {
        private readonly IFinishCriteria finishCriteria;
        readonly CompositeDisposable disposable;
        private TrackOfCheckpoints track;
        readonly Subject<List<RoundPosition>> sequence = new Subject<List<RoundPosition>>();
        public IObservable<List<RoundPosition>> Sequence => sequence;

        public Pipeline(IEnumerable<IObservable<Checkpoint>> checkpointProviders,
            IFinishCriteria finishCriteria,
            ICheckpointAggregator checkpointAggregator = null)
        {
            this.finishCriteria = finishCriteria;
            disposable = new CompositeDisposable(checkpointProviders.Select(x =>
            {
                if (checkpointAggregator == null)
                    return ObservableExtensions.Subscribe<Checkpoint>(x, OnCheckpoint);
                return x.Subscribe(checkpointAggregator);
            }));
            if (checkpointAggregator != null)
                disposable.Add(checkpointAggregator.Subscribe(OnCheckpoint));
        }

        public void StartRound(DateTime roundStartTime)
        {
            track = new TrackOfCheckpoints(roundStartTime, finishCriteria);
        }

        public void StopRound()
        {
            track?.ForceFinish();
        }

        void OnCheckpoint(Checkpoint cp)
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