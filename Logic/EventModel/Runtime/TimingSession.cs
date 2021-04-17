using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.PlatformServices;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.UpstreamData;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    /// SessionDataProvider:
    ///  - Planned start/stop
    ///  - Duration, finish criteria, minimal lap (checkpoint deduplication interval)
    ///  - Notify when data changed
    /// RiderDataManager:
    ///  - Persistent storage of rider data (CRUD)
    ///  - Load and notify updates for timing session
    /// CheckpointService:
    ///  - Store and stream tags
    /// RaceLogService:
    ///  - Final storage of raw checkpoints (RIFD and Number)
    ///  - Manual input of Number checkpoint
    ///  - Checkpoints should have GateId
    /// TimingSessionService:
    ///  - Subscribes to SessionData, RiderData, RaceLog
    ///  - Map RFID/Number checkpoints to Riders
    ///  - Deduplicate checkpoints
    ///  - Incrementally update Rating on each new Checkpoint
    ///  - Recalculate deduplication on update of RiderData or deduplication settings
    ///  - Notify subscribers of new Rating throttled to 500ms
    
    
    
    public class TimingSession : IHasName, IHasTimestamp, IHasSeed
    {
        private readonly Id<TimingSessionDto> id;
        private readonly IEventRepository eventRepository;
        private readonly IRecordingService recordingService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock clock;
        private TimestampAggregator<Checkpoint> checkpointAggregator;

        public Id<SessionDto> SessionId { get; private set; }
        public Id<RecordingSessionDto> RecordingSessionId { get; private set; }
        public DateTime StartTime { get; private set; }
        public List<Checkpoint> RawCheckpoints { get; } = new();
        public List<Checkpoint> AggCheckpoints { get; } = new();
        public ITrackOfCheckpoints Track { get; private set; }
        public ConcurrentDictionary<string, List<Id<RiderProfileDto>>> RiderIdMap { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSeed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public TimingSession(Id<TimingSessionDto> id, IEventRepository eventRepository, IRecordingService recordingService, IMessageHub messageHub, ISystemClock clock)
        {
            this.id = id;
            this.eventRepository = eventRepository;
            this.recordingService = recordingService;
            this.messageHub = messageHub;
            this.clock = clock;
            messageHub.Subscribe<UpstreamDataSyncComplete>(_ => Initialize());
            messageHub.Subscribe<EventDataUpdated>(Initialize);
            Initialize();
        }

        public void Initialize(EventDataUpdated msg = null)
        {
            if (msg != null)
            {
                switch (msg.Entity)
                {
                    case RecordingSessionDto r:
                    case CheckpointDto c:
                        return;
                }
            }
            var timingSession = eventRepository.GetRawDtoById(id);
            var session = eventRepository.GetRawDtoById(timingSession.SessionId);
            var recordingSession = eventRepository.GetRawDtoById(timingSession.RecordingSessionId);
            Track = new TrackOfCheckpoints(StartTime, new FinishCriteria(session.FinishCriteria));
            RawCheckpoints.Clear();
            AggCheckpoints.Clear();
            checkpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(session.MinLap);
            checkpointAggregator.Subscribe(Track.Append);
            checkpointAggregator.AggregatedCheckpoints.Subscribe(AggCheckpoints.Add);
            
            //foreach (var checkpoint in initialCheckpoints) checkpointAggregator.OnNext(ResolveRiderId(checkpoint));
        }

        public void Start()
        {
            eventRepository.Update(id, x =>
            {
                x.Start(clock.UtcNow.UtcDateTime);
            });
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