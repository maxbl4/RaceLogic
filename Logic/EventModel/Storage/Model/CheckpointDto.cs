using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventModel.Storage.Model
{
    public class CheckpointDto : IHasId<CheckpointDto>, IHasSeed, ICheckpoint
    {
        public Id<GateDto> GateId { get; set; }

        public DateTime Timestamp { get; set; }
        public string RiderId { get; set; }
        public DateTime LastSeen { get; set; }
        public int Count { get; set; }
        public bool Aggregated { get; set; }
        public bool IsManual { get; set; }
        public int Rps { get; set; }
        public Id<CheckpointDto> Id { get; set; }
        public bool IsSeed { get; set; }
    }
}