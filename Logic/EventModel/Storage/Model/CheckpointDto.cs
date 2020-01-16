using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.LogManagement.EntryTypes;

namespace maxbl4.Race.Logic.EventStorage.Storage.Model
{
    public class CheckpointDto: IHasId<CheckpointDto>, IHasSeed, ICheckpoint
    {
        private long id;
        public DateTime Timestamp { get; set; }

        long IEntry.Id
        {
            get => id;
            set => id = value;
        }

        public Id<CheckpointDto> Id { get; set; }
        public bool IsSeed { get; set; }
        public string RiderId { get; set; }
    }
}