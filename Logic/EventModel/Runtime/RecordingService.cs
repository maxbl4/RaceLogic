using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class RecordingService : IRecordingService
    {
        private static readonly ILogger logger = Log.ForContext<RecordingService>();
        private readonly IOptions<RecordingServiceOptions> options;
        private readonly IRecordingServiceRepository repository;
        private readonly ICheckpointServiceClientFactory checkpointServiceClientFactory;
        private readonly IAutoMapperProvider mapper;
        private readonly ISystemClock clock;
        private readonly SemaphoreSlim sync = new(1);
        private RecordingSessionDto activeSession;
        private CompositeDisposable disposable;
        private readonly Subject<Checkpoint> checkpoints = new();
        private ICheckpointSubscription subscription;

        public RecordingService(IOptions<RecordingServiceOptions> options, IRecordingServiceRepository repository, ICheckpointServiceClientFactory checkpointServiceClientFactory, IAutoMapperProvider mapper, ISystemClock clock)
        {
            this.options = options;
            this.repository = repository;
            this.checkpointServiceClientFactory = checkpointServiceClientFactory;
            this.mapper = mapper;
            this.clock = clock;
            Initialize();
        }

        private void Initialize()
        {
            using var _ = sync.UseOnce();
            logger.Information("Initialize");
            var activeSession = repository.GetActiveRecordingSession();
            if (activeSession != null)
            {
                logger.Information("Initialize restarting saved session {id}", activeSession.Id);
                Start(activeSession);
            }
        }

        public void StopRecording()
        {
            using var _ = sync.UseOnce();
            if (activeSession != null)
            {
                logger.Information("StopRecording stopping {recordId}", activeSession.Id);
                repository.UpdateRecordingSession(activeSession.Id, x => x.Stop(clock.UtcNow.UtcDateTime));
                disposable.DisposeSafe();
                activeSession = null;
            }
            else
            {
                logger.Information("StopRecording nothing to do");
            }
        }

        public void StartRecording(Id<EventDto> eventId)
        {
            using var _ = sync.UseOnce();
            logger.Information($"StartRecording for event {eventId}", eventId);
            if (activeSession != null && activeSession.EventId == eventId)
            {
                logger.Information($"StartRecording already recording {eventId}", eventId);
                return;
            }
            if (activeSession != null)
            {
                logger.Information($"StartRecording stopping previous record {eventId}", eventId);
                StopRecording();
            }

            var dto = repository.GetSessionForEvent(eventId);
            if (dto == null)
            {
                dto = new RecordingSessionDto
                {
                    EventId = eventId
                };
                dto.Start(clock.UtcNow.UtcDateTime);
                repository.SaveSession(dto);
                logger.Information("StartRecording created new session event={id}, recording={recordId}", eventId, dto.Id);
            }
            Start(dto);
            activeSession = dto;
        }

        private void Start(RecordingSessionDto dto)
        {
            var client = checkpointServiceClientFactory.CreateClient(options.Value.CheckpointServiceAddress);
            disposable = new CompositeDisposable(
                subscription = client.CreateSubscription(dto.StartTime),
                subscription.Checkpoints.Subscribe(OnCheckpoint)
            );
            subscription.Start();
            //client.SetRfidStatus(true);
        }
        
        private void OnCheckpoint(Checkpoint checkpoint)
        {
            using var _ = sync.UseOnce();
            logger.Information("OnCheckpoint {riderId} {timestamp}", checkpoint.RiderId, checkpoint.Timestamp);
            if (activeSession == null) return;
            var dto = mapper.Map<CheckpointDto>(checkpoint);
            dto.RecordingSessionId = activeSession.Id;
            repository.UpsertCheckpoint(dto);
            checkpoints.OnNext(checkpoint);
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer, DateTime from)
        {
            using var _ = sync.UseOnce();
            logger.Information($"Subscribe {observer} {from}", observer.GetType().Name, from);
            if (activeSession == null)
                throw new NotSupportedException("Must have active recording session");
            var cps = mapper.Map<List<Checkpoint>>(repository.GetCheckpoints(activeSession.Id, from));
            var s = cps.ToObservable()
                .ObserveOn(TaskPoolScheduler.Default)
                .Concat(checkpoints)
                .Subscribe(observer);
            disposable.Add(s);
            return s;
        }
    }
    
    // public static class SemaphoreExt
    // {
    //     private static int nextId = 1;
    //     public static IDisposable UseOnce(this SemaphoreSlim semaphore, ILogger logger, [CallerMemberName]string callerName = null)
    //     {
    //         var id = Interlocked.Increment(ref nextId);
    //         logger.Information("Waiting on semaphore [{id}] {callerName}", id, callerName);
    //         if (!semaphore.Wait(5000))
    //             Environment.Exit(100500);
    //         logger.Information("Waiting on semaphore [{id}] {callerName} - success", id, callerName);
    //         return Disposable.Create(() =>
    //         {
    //             logger.Information("Waiting on semaphore [{id}] {callerName} - release", id, callerName);
    //             semaphore.Release();
    //         });
    //     }
    // }
}