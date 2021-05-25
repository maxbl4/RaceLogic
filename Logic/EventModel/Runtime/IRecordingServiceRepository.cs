using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.ServiceBase;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface IRecordingServiceRepository: IRepository
    {
        void SaveSession(RecordingSessionDto dto);
        void UpdateRecordingSession(Id<RecordingSessionDto> id, Action<RecordingSessionDto> modifier);
        void UpsertCheckpoint(CheckpointDto checkpoint);
        IEnumerable<CheckpointDto> GetCheckpoints(Id<GateDto> gateId, DateTime @from, DateTime to);
        IEnumerable<RecordingSessionDto> GetActiveSessions();
        RecordingSessionDto GetActiveSessionForGate(Id<GateDto> gateId);
    }
}