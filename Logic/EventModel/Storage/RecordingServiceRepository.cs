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

        public RecordingSessionDto GetSessionForGate(Id<GateDto> gateId)
        {
            var dto = StorageService.Repo.FirstOrDefault<RecordingSessionDto>(x => x.GateId == gateId && x.IsRunning);
            if (dto == null)
            {
                dto = new RecordingSessionDto
                {
                    GateId = gateId
                };
                dto.Start(clock.UtcNow.UtcDateTime);
                StorageService.Save(dto);
            }
            return dto;
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

        public IEnumerable<CheckpointDto> GetCheckpoints(Id<RecordingSessionDto> sessionId, DateTime from)
        {
            return StorageService.Repo.Query<CheckpointDto>()
                .Where(x => x.RecordingSessionId == sessionId && x.Timestamp >= from)
                .ToEnumerable()
                .OrderBy(x => x.Timestamp);
        }

        public IStorageService StorageService { get; }

        private void SetupIndexes(ILiteRepository repo)
        {
            repo.Database.GetCollection<RecordingSessionDto>().EnsureIndex(x => x.IsRunning);
            repo.Database.GetCollection<CheckpointDto>().EnsureIndex(x => x.RecordingSessionId);
        }
    }
}