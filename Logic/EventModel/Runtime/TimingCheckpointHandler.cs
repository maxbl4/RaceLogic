using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime;

public class TimingCheckpointHandler: IDisposable
{
    public Id<TimingSessionDto> TimingSessionId { get; }
    private readonly CompositeDisposable disposable;
    public ReadOnlyDictionary<string, List<Id<RiderClassRegistrationDto>>> RiderIdMap { get; }
    
    public TimingCheckpointHandler(DateTime startTime, Id<TimingSessionDto> timingSessionId, SessionDto session, 
        IDictionary<string, List<Id<RiderClassRegistrationDto>>> riderIdMap)
    {
        this.TimingSessionId = timingSessionId;
        RiderIdMap = new ReadOnlyDictionary<string, List<Id<RiderClassRegistrationDto>>>(riderIdMap);
        disposable = new CompositeDisposable();
        Track = new TrackOfCheckpoints(startTime, new FinishCriteria(session.FinishCriteria));
        CheckpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(session.MinLap);
        disposable.Add(CheckpointAggregator.Subscribe(Track.Append));
    }

    public TimestampAggregator<Checkpoint> CheckpointAggregator { get; }

    public TrackOfCheckpoints Track { get; }

    public void AppendCheckpoint(Checkpoint cp)
    {
        ResolveRiderId(cp, CheckpointAggregator);
    }

    private void ResolveRiderId(Checkpoint rawCheckpoint, IObserver<Checkpoint> observer)
    {
        if (RiderIdMap.TryGetValue(rawCheckpoint.RiderId, out var riderIds))
        {
            foreach (var riderId in riderIds)
            {
                observer.OnNext(rawCheckpoint.WithRiderId(riderId.ToString()));
            }
        }
        else
        {
            observer.OnNext(rawCheckpoint.WithRiderId(rawCheckpoint.RiderId));
        }
    }

    public void Dispose()
    {
        disposable.DisposeSafe();
    }
}