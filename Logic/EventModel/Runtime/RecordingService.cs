using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.SemaphoreExt;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class RecordingService
    {
        private readonly IRecordingServiceStorage storage;
        private readonly ICheckpointServiceClientFactory checkpointServiceClientFactory;
        private readonly IAutoMapperProvider mapper;
        private RecordingSession activeSession;

        public RecordingService(IRecordingServiceStorage storage, ICheckpointServiceClientFactory checkpointServiceClientFactory, IAutoMapperProvider mapper)
        {
            this.storage = storage;
            this.checkpointServiceClientFactory = checkpointServiceClientFactory;
            this.mapper = mapper;
            Initialize();
        }

        private void Initialize()
        {
            var dto = storage.GetActiveSession();
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

        private RecordingSession Create(RecordingSessionDto dto)
        {
            var client = dto.CheckpointServiceAddress != null ? checkpointServiceClientFactory.CreateClient(dto.CheckpointServiceAddress) : null;
            return activeSession = new RecordingSession(dto.Id, client, storage, mapper);
        }
    }

    public class RecordingSession : IDisposable, IObservable<Checkpoint>
    {
        private readonly ICheckpointServiceClient client;
        private readonly IRecordingServiceStorage storage;
        private readonly IAutoMapperProvider mapper;
        private ICheckpointSubscription subscription;
        private readonly CompositeDisposable disposable;
        private readonly Subject<Checkpoint> chekpoints = new();
        private readonly SemaphoreSlim sync = new(1);

        public RecordingSession(Id<RecordingSessionDto> id, ICheckpointServiceClient client, IRecordingServiceStorage storage, IAutoMapperProvider mapper)
        {
            this.client = client;
            this.storage = storage;
            this.mapper = mapper;
            SessionId = id;
            var dto = storage.GetSession(id);
            if (client != null)
            {
                disposable = new CompositeDisposable(
                    subscription = client.CreateSubscription(dto.StartTime),
                    subscription.Checkpoints.Subscribe(OnCheckpoint)
                    );
                subscription.Start();
            }
        }

        public Id<RecordingSessionDto> SessionId { get; }

        public void Dispose()
        {
            disposable.DisposeSafe();
        }

        public void Stop()
        {
            disposable.DisposeSafe();
            storage.UpdateSession(dto => dto.IsRunning = false);
        }

        private void OnCheckpoint(Checkpoint checkpoint)
        {
            using var _ = sync.UseOnce();
            var dto = mapper.Map<CheckpointDto>(checkpoint);
            dto.RecordingSessionId = SessionId;
            storage.UpsertCheckpoint(dto);
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer)
        {
            using var _ = sync.UseOnce();
            var cps = mapper.Map<List<Checkpoint>>(storage.GetCheckpoints(SessionId));
            cps.ToObservable()

                .Buffer(10)
                .SelectMany(x => x)
                .Concat(chekpoints);
            
            return null;
        }
    }

    public interface IRecordingServiceStorage
    {
        RecordingSessionDto GetActiveSession();
        RecordingSessionDto GetSession(Id<RecordingSessionDto> sessionId);
        void SaveSession(RecordingSessionDto dto);
        void UpdateSession(Action<RecordingSessionDto> modifier);
        void UpsertCheckpoint(CheckpointDto checkpoint);
        IEnumerable<CheckpointDto> GetCheckpoints(Id<RecordingSessionDto> sessionId);
    }
}