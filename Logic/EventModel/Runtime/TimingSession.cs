using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSession: IHasName, IHasTimestamp, IHasSeed
    {
        private SemaphoreSlim sync = new SemaphoreSlim(1);
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
     
        public Id<SessionDto> SessionDtoId { get; set; }
        public Id<RecordingSessionDto> RecordingSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan MinLap { get; set; } = TimeSpan.FromSeconds(15);
        public List<Checkpoint> RawCheckpoints { get; } = new List<Checkpoint>();
        public List<Checkpoint> AggCheckpoints { get; } = new List<Checkpoint>();
        public IFinishCriteria FinishCriteria { get; set; }
        public ITrackOfCheckpoints Track { get; private set; }
        public TimestampAggregator<Checkpoint> checkpointAggregator;
        public ConcurrentDictionary<string, List<Id<RiderProfileDto>>> RiderIdMap { get; set; }
        
        public void Initialize(IEnumerable<Checkpoint> initialCheckpoints = null)
        {
            using var s = sync.UseOnce();
            Track = new TrackOfCheckpoints(StartTime, FinishCriteria);
            RawCheckpoints.Clear();
            AggCheckpoints.Clear();
            checkpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(MinLap);
            checkpointAggregator.Subscribe(Track.Append);
            checkpointAggregator.AggregatedCheckpoints.Subscribe(AggCheckpoints.Add);
            foreach (var checkpoint in initialCheckpoints)
            {
                checkpointAggregator.OnNext(ResolveRiderId(checkpoint));
            }
        }

        public void AppendCheckpoint(Checkpoint checkpoint)
        {
            using var s = sync.UseOnce();
            RawCheckpoints.Add(checkpoint);
            checkpointAggregator.OnNext(ResolveRiderId(checkpoint));
        }

        private Checkpoint ResolveRiderId(Checkpoint rawCheckpoint)
        {
            var resolvedId = RiderIdMap.TryGetValue(rawCheckpoint.RiderId, out var riderId) ? riderId.ToString() : rawCheckpoint.RiderId;
            return rawCheckpoint.WithRiderId(resolvedId);
        }
    }
}