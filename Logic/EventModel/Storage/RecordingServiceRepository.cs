using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.PlatformServices;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class RecordingServiceRepository: IRecordingServiceRepository
    {
        private readonly ISystemClock clock;

        public RecordingServiceRepository(IStorageService storageService, ISystemClock clock)
        {
            this.clock = clock;
            StorageService = storageService;
            SetupIndexes(storageService.Repo);
        }
        
        public IEnumerable<RecordingSessionDto> GetActiveSessions()
        {
            return StorageService.List<RecordingSessionDto>(x => x.IsRunning);
        }

        public RecordingSessionDto GetActiveSessionForGate(Id<GateDto> gateId)
        {
            return StorageService.Repo.FirstOrDefault<RecordingSessionDto>(x => x.IsRunning && x.GateId == gateId);
        }

        public void SaveSession(RecordingSessionDto dto)
        {
            StorageService.Save(dto);
        }

        public void UpdateRecordingSession(Id<RecordingSessionDto> id, Action<RecordingSessionDto> modifier)
        {
            StorageService.Update(id, modifier);
        }

        public void UpsertCheckpoint(CheckpointDto checkpoint)
        {
            StorageService.Save(checkpoint);
        }

        public IEnumerable<CheckpointDto> GetCheckpoints(Id<GateDto> gateId, DateTime from, DateTime to)
        {
            return StorageService.Repo.Query<CheckpointDto>()
                .Where(x => x.GateId == gateId && !x.Aggregated && x.Timestamp > from && x.Timestamp <= to)
                .ToEnumerable()
                .OrderBy(x => x.Timestamp);
        }

        public IStorageService StorageService { get; }

        private void SetupIndexes(ILiteRepository repo)
        {
            repo.Database.GetCollection<RecordingSessionDto>().EnsureIndex(x => x.IsRunning);
            repo.Database.GetCollection<RecordingSessionDto>().EnsureIndex(x => x.GateId);
            repo.Database.GetCollection<CheckpointDto>().EnsureIndex(x => x.GateId);
            repo.Database.GetCollection<CheckpointDto>().EnsureIndex(x => x.Aggregated);
        }
    }
}