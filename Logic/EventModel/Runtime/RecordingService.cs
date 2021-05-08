using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface IRecordingService
    {
        RecordingSession StartRecordingSession(string name, string checkpointServiceAddress, bool createNew = false);
        RecordingSession GetOrCreateRecordingSession(Id<RecordingSessionDto> id);
    }

    public class RecordingService : IRecordingService
    {
        private readonly IRecordingServiceStorage storage;
        private readonly ICheckpointServiceClientFactory checkpointServiceClientFactory;
        private readonly IAutoMapperProvider mapper;
        private readonly ISystemClock clock;
        private RecordingSession activeSession;

        public RecordingService(IRecordingServiceStorage storage, ICheckpointServiceClientFactory checkpointServiceClientFactory, IAutoMapperProvider mapper, ISystemClock clock)
        {
            this.storage = storage;
            this.checkpointServiceClientFactory = checkpointServiceClientFactory;
            this.mapper = mapper;
            this.clock = clock;
            Initialize();
        }

        private void Initialize()
        {
            var dto = storage.GetActiveRecordingSession();
            if (dto != null)
                activeSession = Create(dto);
        }

        public RecordingSession StartRecordingSession(string name, string checkpointServiceAddress, bool createNew = false)
        {
            if (!createNew && activeSession != null)
                return activeSession;
            if (activeSession != null)
            {
                activeSession.Dispose();
                activeSession = null;
            }

            var dto = new RecordingSessionDto
            {
                Name = name,
                CheckpointServiceAddress = checkpointServiceAddress,
                IsRunning = true,
                StartTime = DateTime.UtcNow
            };
            storage.SaveSession(dto);
            return activeSession = Create(dto);
        }

        public RecordingSession GetOrCreateRecordingSession(Id<RecordingSessionDto> id)
        {
            var dto = storage.GetSession(id);
            if (dto == null)
            {
                
            }

            return Create(dto);
        }

        private RecordingSession Create(RecordingSessionDto dto)
        {
            var client = dto.CheckpointServiceAddress != null ? checkpointServiceClientFactory.CreateClient(dto.CheckpointServiceAddress) : null;
            return activeSession = new RecordingSession(dto.Id, client, storage, mapper, clock);
        }
    }

    public class RecordingSession : IDisposable, IObservable<Checkpoint>
    {
        private readonly ICheckpointServiceClient client;
        private readonly IRecordingServiceStorage storage;
        private readonly IAutoMapperProvider mapper;
        private readonly ISystemClock clock;
        private ICheckpointSubscription subscription;
        private readonly CompositeDisposable disposable;
        private readonly Subject<Checkpoint> chekpoints = new();
        private readonly SemaphoreSlim sync = new(1);

        public RecordingSession(Id<RecordingSessionDto> id, ICheckpointServiceClient client, IRecordingServiceStorage storage, IAutoMapperProvider mapper, ISystemClock clock)
        {
            this.client = client;
            this.storage = storage;
            this.mapper = mapper;
            this.clock = clock;
            Id = id;
            var dto = storage.GetSession(id);
            if (client != null)
            {
                storage.UpdateRecordingSession(Id, dto =>
                {
                    dto.Start(clock.UtcNow.UtcDateTime);
                });
                disposable = new CompositeDisposable(
                    subscription = client.CreateSubscription(dto.StartTime),
                    subscription.Checkpoints.Subscribe(OnCheckpoint)
                    );
                subscription.Start();
            }
        }

        public Id<RecordingSessionDto> Id { get; }

        public void Dispose()
        {
            disposable.DisposeSafe();
            chekpoints.DisposeSafe();
        }

        public void Stop()
        {
            disposable.DisposeSafe();
            storage.UpdateRecordingSession(this.Id, dto =>
            {
                dto.Stop(clock.UtcNow.UtcDateTime);
            });
        }

        private void OnCheckpoint(Checkpoint checkpoint)
        {
            using var _ = sync.UseOnce();
            var dto = mapper.Map<CheckpointDto>(checkpoint);
            dto.RecordingSessionId = Id;
            storage.UpsertCheckpoint(dto);
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer)
        {
            using var _ = sync.UseOnce();
            var cps = mapper.Map<List<Checkpoint>>(storage.GetCheckpoints(Id));
            return cps.ToObservable()
                .ObserveOn(TaskPoolScheduler.Default)
                .Concat(chekpoints)
                .Subscribe(observer);
        }
    }

    public interface IRecordingServiceStorage
    {
        RecordingSessionDto GetActiveRecordingSession();
        RecordingSessionDto GetSession(Id<RecordingSessionDto> sessionId);
        void SaveSession(RecordingSessionDto dto);
        void UpdateRecordingSession(Id<RecordingSessionDto> id, Action<RecordingSessionDto> modifier);
        void UpsertCheckpoint(CheckpointDto checkpoint);
        IEnumerable<CheckpointDto> GetCheckpoints(Id<RecordingSessionDto> sessionId);
    }
}