using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;

namespace maxbl4.Race.Logic.EventModel.Runtime
{
    public interface IRecordingService
    {
        void Initialize();
        void StartRfid(Id<GateDto> gateId);
        void StopRfid(Id<GateDto> gateId);
        void AppendCheckpoint(Id<GateDto> gateId, Checkpoint checkpoint);
    }
}