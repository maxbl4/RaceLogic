using System;
using System.Threading.Tasks;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public class RecordingService
    {
        private readonly IRecordingServiceStorage storage;
        private RecordingSession actionSession;

        public RecordingService(IRecordingServiceStorage storage)
        {
            this.storage = storage;
        }

        public async Task Initialize()
        {
            var dto = await storage.GetActiveSession();
            if (dto != null)
                actionSession = new RecordingSession(null, storage, dto);
        }

        /// <summary>
        ///     Start new session. Returns currently active session if createNew == false
        /// </summary>
        /// <returns></returns>
        public async Task<RecordingSession> StartRecordingSession(bool createNew = false)
        {
            if (!createNew && actionSession != null)
                return actionSession;
            if (actionSession != null)
            {
                await actionSession.DisposeAsync();
                actionSession = null;
            }

            actionSession = new RecordingSession(null, storage, new RecordingSessionDto());
            return actionSession;
        }

        /// <summary>
        ///     Load and continue previous session or return e
        /// </summary>
        /// <param name="recordingSessionId"></param>
        /// <returns></returns>
        public async Task<RecordingSession> ContinueRecordingSession(Guid recordingSessionId)
        {
            if (actionSession != null && actionSession.SessionId == recordingSessionId)
                return actionSession;
            actionSession = new RecordingSession(null, storage, await storage.GetSession(recordingSessionId));
            return actionSession;
        }
    }

    public class RecordingSession : IAsyncDisposable
    {
        private readonly ICheckpointServiceClientFactory cpFactory;
        private readonly IRecordingServiceStorage storage;

        public RecordingSession(ICheckpointServiceClientFactory cpFactory, IRecordingServiceStorage storage,
            RecordingSessionDto dto)
        {
            this.cpFactory = cpFactory;
            this.storage = storage;
            SessionId = dto.Id;
        }

        public Guid SessionId { get; }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public async Task Start()
        {
            // TODO: Subscribe to checkpoints from each enabled CheckpointService
            // Call OnCheckpoint
            await Task.Delay(1);
        }

        public async Task Stop()
        {
            // TODO: dispose subscription
            await Task.Delay(1);
        }

        private void OnCheckpoint(Checkpoint checkpoint)
        {
            // TODO: store checkpoint and signal we have updates
        }
    }

    public interface ICheckpointServiceClient
    {
    }

    public interface ICheckpointServiceClientFactory
    {
        ICheckpointServiceClient CreateClient(string address);
    }

    public interface IRecordingServiceStorage
    {
        Task<RecordingSessionDto> GetActiveSession();
        Task<RecordingSessionDto> GetSession(Guid sessionId);
        Task<string> GetCheckpointServiceAddress();
        Task SaveSession(RecordingSessionDto dto);
    }
}