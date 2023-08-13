using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.WebModel;
using Serilog;
using RoundPosition = maxbl4.Race.Logic.RoundTiming.RoundPosition;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class TimingSession: IDisposable
    {
        private static readonly ILogger logger = Log.ForContext<TimingSession>(); 
        public Id<TimingSessionDto> Id { get; }
        public Id<SessionDto> SessionId { get; }
        private readonly ICheckpointStorage checkpointRepository;
        private readonly IEventRepository eventRepository;
        private readonly IMessageHub messageHub;
        private readonly IAutoMapperProvider autoMapperProvider;
        private CompositeDisposable disposable;
        private TimingCheckpointHandler checkpointHandler;
        private readonly SyncLock sync = new();
        
        public List<RoundPosition> Rating => checkpointHandler.Track.Rating;

        public TimingSession(Id<TimingSessionDto> id,
            Id<SessionDto> sessionId,
            ICheckpointStorage checkpointRepository,
            IEventRepository eventRepository,
            IMessageHub messageHub, IAutoMapperProvider autoMapperProvider)
        {
            this.Id = id;
            this.SessionId = sessionId;
            this.checkpointRepository = checkpointRepository;
            this.eventRepository = eventRepository;
            this.messageHub = messageHub;
            this.autoMapperProvider = autoMapperProvider;
            Reload();
        }

        public void Reload()
        {
            using var _ = sync.Use();
            disposable?.DisposeSafe();
            
            disposable = new CompositeDisposable();
            var timingSession = eventRepository.StorageService.Get(Id);
            logger.Information("Activating {name} {id}", timingSession.Name, Id);
            
            var session = eventRepository.GetWithUpstream(timingSession.SessionId);
            disposable.Add(checkpointHandler = new TimingCheckpointHandler(timingSession.StartTime, Id, session,
                eventRepository.GetRiderIdentifiers(timingSession.SessionId)));

            messageHub.Publish(new RiderEventInfoUpdate
            {
                TimingSessionId = Id,
                Riders = eventRepository.ListRiderEventInfo(Id) 
            });
            
            var ratingUpdates = new Subject<RatingUpdatedMessage>();
            disposable.Add(ratingUpdates);
            disposable.Add(ratingUpdates
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(r =>
                {
                    messageHub.Publish(r);
                    var update = TimingSessionUpdate.From(this.Id, this.Rating, autoMapperProvider);
                    messageHub.Publish(update);
                }));
            var cpSubject = new Subject<Checkpoint>();
            disposable.Add(cpSubject);
            disposable.Add(messageHub.Subscribe<Checkpoint>(cpSubject.OnNext));

            var cps = checkpointRepository.ListCheckpoints(timingSession.StartTime, timingSession.StopTime);
            foreach (var cp in cps)
            {
                checkpointHandler.AppendCheckpoint(cp);
            }
            disposable.Add(cpSubject
                .Subscribe(cp =>
                {
                    checkpointHandler.AppendCheckpoint(cp);
                    ratingUpdates.OnNext(new RatingUpdatedMessage(checkpointHandler.Track.Rating, Id));    
                }));
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
            checkpointHandler.DisposeSafe();
        }
    }
}