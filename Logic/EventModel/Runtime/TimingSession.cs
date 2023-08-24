using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Infrastructure.Extensions.DisposableExt;
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
        private Dictionary<string,Rider> riderLookup;

        public List<RoundPosition> Rating => checkpointHandler.Track.Rating;
        public List<Checkpoint> Checkpoints => checkpointHandler.Track.Checkpoints;

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
        }

        public void Start()
        {
            Reload();
        }

        public void Reload(bool subscribeToRealtimeData = true)
        {
            using var _ = sync.Use();
            disposable?.DisposeSafe();
            
            disposable = new CompositeDisposable();
            var timingSession = eventRepository.StorageService.Get(Id);
            logger.Information("Activating {name} {id}", timingSession.Name, Id);

            var riderIdMap = eventRepository.GetRidersWithIdentifiers(timingSession.SessionId);
            CreateRiderIdLookups(riderIdMap);
            var session = eventRepository.GetWithUpstream(timingSession.SessionId);
            disposable.Add(checkpointHandler = new TimingCheckpointHandler(timingSession.StartTime, Id, session,
                riderIdMap));
            
            var ratingUpdates = new Subject<RatingUpdatedMessage>();
            disposable.Add(ratingUpdates);
            disposable.Add(ratingUpdates
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(r =>
                {
                    try
                    {
                        messageHub.Publish(r);
                        messageHub.Publish(GetTimingSessionUpdate());
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Publish session update failed");
                    }
                }));
            
            var cps = checkpointRepository.ListCheckpoints(timingSession.StartTime, timingSession.StopTime)
                .Where(x => !x.Aggregated);
            foreach (var cp in cps)
            {
                checkpointHandler.AppendCheckpoint(cp);
            }
            
            if (subscribeToRealtimeData)
            {
                disposable.Add(checkpointHandler.TrackUpdated.Subscribe(_ =>
                {
                    ratingUpdates.OnNext(new RatingUpdatedMessage(checkpointHandler.Track.Rating, Id));
                }));
                var cpSubject = new Subject<Checkpoint>();
                disposable.Add(cpSubject);
                disposable.Add(messageHub.Subscribe<Checkpoint>(cp =>
                {
                    if (!cp.Aggregated) cpSubject.OnNext(cp);
                }));
                disposable.Add(cpSubject
                    .Subscribe(checkpointHandler.AppendCheckpoint));
            }
        }

        private void CreateRiderIdLookups(Dictionary<string, List<RiderClassRegistrationDto>> riderIdMap)
        {
            var session = eventRepository.GetWithUpstream(SessionId);
            var classes = session.ClassIds.Select(x => eventRepository.GetWithUpstream(x))
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.First());
            riderLookup = riderIdMap.Values.SelectMany(x => x)
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key.ToString(), x =>
                {
                    var r = x.First();
                    return new Rider
                    {
                        Id = r.Id.ToString(),
                        Class = new Class
                        {
                            Id = r.ClassId.ToString(),
                            Name = classes.Get(r.ClassId)?.Name ?? "<НЕТ>"
                        },
                        FirstName = r.FirstName,
                        ParentName = r.ParentName,
                        LastName = r.LastName,
                        Number = r.Number.ToString()
                    };
                });
        }

        public TimingSessionUpdate GetTimingSessionUpdate()
        { 
            var update = new TimingSessionUpdate
            {
                Id = Id.Value,
                TimingSessionId = Id,
                Rating = Rating.Select(MapRating).ToList(),
                ResolvedCheckpoints = Checkpoints,
                Riders = riderLookup.Values.ToList(),
                MaxLapCount = Rating.Count > 0 ? Rating.Max(x => x.LapCount) : 0
            };
            return update;
        }

        private WebModel.RoundPosition MapRating(RoundPosition o)
        {
            var p = autoMapperProvider.Map<WebModel.RoundPosition>(o);
            p.RiderId = o.RiderId;
            p.Rider = riderLookup.Get(o.RiderId) ?? new Rider{Number = o.RiderId,LastName = "#", FirstName = o.RiderId};
            return p;
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
        }
    }
}