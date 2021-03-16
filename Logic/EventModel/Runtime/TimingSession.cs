using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSession : IHasName, IHasTimestamp, IHasSeed
    {
        public TimestampAggregator<Checkpoint> checkpointAggregator;

        public Id<SessionDto> SessionId { get; set; }
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public List<Checkpoint> RawCheckpoints { get; } = new();
        public List<Checkpoint> AggCheckpoints { get; } = new();
        public IFinishCriteria FinishCriteria { get; set; }
        public ITrackOfCheckpoints Track { get; private set; }
        public ConcurrentDictionary<string, List<Id<RiderProfileDto>>> RiderIdMap { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public void Initialize(IEnumerable<Checkpoint> initialCheckpoints = null)
        {
            Track = new TrackOfCheckpoints(StartTime, FinishCriteria);
            RawCheckpoints.Clear();
            AggCheckpoints.Clear();
            checkpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(MinLap);
            checkpointAggregator.Subscribe(Track.Append);
            checkpointAggregator.AggregatedCheckpoints.Subscribe(AggCheckpoints.Add);
            foreach (var checkpoint in initialCheckpoints) checkpointAggregator.OnNext(ResolveRiderId(checkpoint));
        }

        public void AppendCheckpoint(Checkpoint checkpoint)
        {
            RawCheckpoints.Add(checkpoint);
            checkpointAggregator.OnNext(ResolveRiderId(checkpoint));
        }

        private Checkpoint ResolveRiderId(Checkpoint rawCheckpoint)
        {
            var resolvedId = RiderIdMap.TryGetValue(rawCheckpoint.RiderId, out var riderId)
                ? riderId.ToString()
                : rawCheckpoint.RiderId;
            return rawCheckpoint.WithRiderId(resolvedId);
        }
    }
}