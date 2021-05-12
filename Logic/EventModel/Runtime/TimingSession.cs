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
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using Serilog;

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
        private static readonly ILogger logger = Log.ForContext<TimingSession>(); 
        public Id<TimingSessionDto> Id { get; set; }
        private readonly IEventRepository eventRepository;
        private readonly IRecordingService recordingService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock clock;
        private TimestampAggregator<Checkpoint> checkpointAggregator;

        public Id<SessionDto> SessionId { get; private set; }
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
            this.Id = id;
            this.eventRepository = eventRepository;
            this.recordingService = recordingService;
            this.messageHub = messageHub;
            this.clock = clock;
            messageHub.Subscribe<UpstreamDataSyncComplete>(_ => Initialize());
            messageHub.Subscribe<StorageUpdated>(Initialize);
            Initialize();
        }

        public void Initialize(StorageUpdated msg = null)
        {
            logger.Information("Initialize {EntityType}", msg?.Entity?.GetType()?.Name);
            if (msg != null)
            {
                switch (msg.Entity)
                {
                    //case RecordingSessionDto:
                    case CheckpointDto:
                    //case TimingSessionDto:
                        return;
                }
            }
            var timingSession = eventRepository.StorageService.Get(Id);
            if (!timingSession.IsRunning)
                return;
            logger.Information("Initialize updating subscription");
            var session = eventRepository.GetWithUpstream(timingSession.SessionId);
            recordingService.StartRecording(session.EventId);
            Track = new TrackOfCheckpoints(StartTime, new FinishCriteria(session.FinishCriteria));
            RawCheckpoints.Clear();
            AggCheckpoints.Clear();
            checkpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(session.MinLap);
            checkpointAggregator.Subscribe(Track.Append);
            checkpointAggregator.AggregatedCheckpoints.Subscribe(AggCheckpoints.Add);
            recordingService.Subscribe(checkpointAggregator, timingSession.StartTime);
        }

        public void Start(DateTime? startTime = null)
        {
            logger.Information("Start");
            eventRepository.StorageService.Update(Id, x =>
            {
                x.Start(startTime ?? clock.UtcNow.UtcDateTime);
            });
        }

        public void AppendCheckpoint(Checkpoint checkpoint)
        {
            logger.Information("AppendCheckpoint");
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