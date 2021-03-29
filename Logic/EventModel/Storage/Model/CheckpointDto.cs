using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class CheckpointDto : IHasId<CheckpointDto>, IHasSeed, ICheckpoint
    {
        public Guid RecordingSessionId { get; set; }

        public DateTime Timestamp { get; set; }
        public string RiderId { get; set; }
        public DateTime LastSeen { get; set; }
        public int Count { get; set; }
        public bool Aggregated { get; set; }
        public bool IsManual { get; set; }
        public int Rps { get; set; }
        public Guid Id { get; set; }
        public bool IsSeed { get; set; }
    }
}