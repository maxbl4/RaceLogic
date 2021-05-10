using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public class RecordingServiceRepository: IRecordingServiceRepository
    {
        public RecordingServiceRepository(IStorageService storageService)
        {
            StorageService = storageService;
            SetupIndexes(storageService.Repo);
        }
        
        public RecordingSessionDto GetActiveRecordingSession()
        {
            return StorageService.Repo.Query<RecordingSessionDto>().Where(x => x.IsRunning).FirstOrDefault();
        }

        public RecordingSessionDto GetSession(Id<RecordingSessionDto> sessionId)
        {
            return StorageService.Get(sessionId);
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

        public IEnumerable<CheckpointDto> GetCheckpoints(Id<RecordingSessionDto> sessionId)
        {
            return StorageService.Repo.Query<CheckpointDto>()
                .Where(x => x.RecordingSessionId == sessionId)
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