using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
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

        public RecordingSessionDto GetSessionForEvent(Id<EventDto> eventId)
        {
            return StorageService.Repo.FirstOrDefault<RecordingSessionDto>(x => x.EventId == eventId);
        }

        public IEnumerable<RecordingSessionDto> ListSessions(Id<EventDto> eventId)
        {
            return StorageService.List<RecordingSessionDto>(x => x.EventId == eventId);
        }

        public RecordingSessionDto GetOrCreateRecordingSession(Id<EventDto> eventId)
        {
            return StorageService.Repo.FirstOrDefault<RecordingSessionDto>(x => x.EventId == eventId);
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