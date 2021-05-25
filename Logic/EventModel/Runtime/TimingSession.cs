using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.RoundTiming;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using Serilog;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public record RatingUpdatedMessage(List<RoundPosition> Rating, Id<TimingSessionDto> TimingSessionId);
    
    public class TimingCheckpointHandler: IDisposable
    {
        private readonly Id<TimingSessionDto> timingSessionId;
        private readonly IMessageHub messageHub;
        private readonly CompositeDisposable disposable;
        private Subject<RatingUpdatedMessage> ratingUpdates = new();
        public ReadOnlyDictionary<string, List<Id<RiderClassRegistrationDto>>> RiderIdMap { get; }
        public TimingCheckpointHandler(DateTime startTime, Id<TimingSessionDto> timingSessionId, SessionDto session, 
            IDictionary<string, List<Id<RiderClassRegistrationDto>>> riderIdMap, IMessageHub messageHub)
        {
            this.timingSessionId = timingSessionId;
            this.messageHub = messageHub;
            RiderIdMap = new ReadOnlyDictionary<string, List<Id<RiderClassRegistrationDto>>>(riderIdMap);
            disposable = new CompositeDisposable();
            Track = new TrackOfCheckpoints(startTime, new FinishCriteria(session.FinishCriteria));
            CheckpointAggregator = TimestampAggregatorConfigurations.ForCheckpoint(session.MinLap);
            disposable.Add(CheckpointAggregator.Subscribe(Track.Append));
            ratingUpdates
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(messageHub.Publish);
        }

        public TimestampAggregator<Checkpoint> CheckpointAggregator { get; }

        public TrackOfCheckpoints Track { get; }

        public void AppendCheckpoint(Checkpoint cp)
        {
            CheckpointAggregator.OnNext(cp);
            ratingUpdates.OnNext(new RatingUpdatedMessage(Track.Rating, timingSessionId));
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

    public class LiveCheckpointFeedSubscription : IDisposable
    {
        private readonly CompositeDisposable disposable = new();
        private readonly SemaphoreSlim sync = new(1);

        public LiveCheckpointFeedSubscription(IObserver<CheckpointDto> observer, IRecordingServiceRepository repo, Id<GateDto> gateId, DateTime from, IMessageHub messageHub)
        {
            using var _ = sync.UseOnce();
            disposable.Add(messageHub.Subscribe<CheckpointDto>(x =>
                {
                    using var _ = sync.UseOnce();
                    observer.OnNext(x);
                }));
            var existing = repo.GetCheckpoints(gateId, from, DateTime.MaxValue);
            foreach (var cp in existing)
            {
                observer.OnNext(cp);
            }
        }
        
        public void Dispose()
        {
            disposable.DisposeSafe();
        }
    }
    
    public class TimingSession
    {
        private static readonly ILogger logger = Log.ForContext<TimingSession>(); 
        public Id<TimingSessionDto> Id { get; }
        private readonly IEventRepository eventRepository;
        private readonly IRecordingService recordingService;
        private readonly IRecordingServiceRepository recordingServiceRepository;
        private readonly IAutoMapperProvider autoMapper;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock clock;
        private CompositeDisposable disposable;
        private TimingCheckpointHandler checkpointHandler;
        public bool UseRfid { get; set; }

        public TimingSession(Id<TimingSessionDto> id, 
            IEventRepository eventRepository, 
            IRecordingService recordingService, IRecordingServiceRepository recordingServiceRepository,
            IAutoMapperProvider autoMapper,
            IMessageHub messageHub, ISystemClock clock)
        {
            this.Id = id;
            this.eventRepository = eventRepository;
            this.recordingService = recordingService;
            this.recordingServiceRepository = recordingServiceRepository;
            this.autoMapper = autoMapper;
            this.messageHub = messageHub;
            this.clock = clock;
            messageHub.Subscribe<UpstreamDataSyncComplete>(_ => Reload());
            messageHub.Subscribe<StorageUpdated>(x => Reload(false, x));
        }

        public void Reload()
        {
            Reload(true);
        }

        private void Reload(bool force, StorageUpdated msg = null)
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
            if (!timingSession.IsRunning && !force)
                return;
            logger.Information("Initialize updating subscription");
            disposable?.DisposeSafe();
            disposable = new CompositeDisposable();
            var session = eventRepository.GetWithUpstream(timingSession.SessionId);
            GateId = eventRepository.GetGateId(timingSession.SessionId);
            checkpointHandler = new TimingCheckpointHandler(timingSession.StartTime, Id, session,
                eventRepository.GetRiderIdentifiers(timingSession.SessionId), messageHub);
            disposable.Add(checkpointHandler);
            var cps = recordingServiceRepository
                .GetCheckpoints(timingSession.GateId, timingSession.StartTime, timingSession.StopTime);
            foreach (var cp in cps)
            {
                recordingService.AppendCheckpoint(GateId, autoMapper.Map<Checkpoint>(cp));
            }
        }

        public Id<GateDto> GateId { get; set; }

        public void Start(DateTime? startTime = null)
        {
            logger.Information("Start");
            eventRepository.StorageService.Update(Id, x =>
            {
                x.Start(startTime ?? clock.UtcNow.UtcDateTime);
            });
        }

        public void ManualCheckpoint(Checkpoint checkpoint)
        {
            logger.Information("ManualCheckpoint");
            recordingService.AppendCheckpoint(GateId, checkpoint);
        }
    }
}