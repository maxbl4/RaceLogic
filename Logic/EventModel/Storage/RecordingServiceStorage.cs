using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public partial class LiteDbEventRepository: IRecordingServiceStorage
    {
        public RecordingSessionDto GetActiveRecordingSession()
        {
            return repo.Query<RecordingSessionDto>().Where(x => x.IsRunning).FirstOrDefault();
        }

        public RecordingSessionDto GetSession(Id<RecordingSessionDto> sessionId)
        {
            return GetRawDtoById(sessionId);
        }

        public void SaveSession(RecordingSessionDto dto)
        {
            Save(dto);
        }

        public void UpdateRecordingSession(Id<RecordingSessionDto> id, Action<RecordingSessionDto> modifier)
        {
            Update(id, modifier);
        }

        public void UpsertCheckpoint(CheckpointDto checkpoint)
        {
            Save(checkpoint);
        }

        public IEnumerable<CheckpointDto> GetCheckpoints(Id<RecordingSessionDto> sessionId)
        {
            return repo.Query<CheckpointDto>()
                .Where(x => x.RecordingSessionId == sessionId)
                .ToEnumerable()
                .OrderBy(x => x.Timestamp);
        }
    }
}