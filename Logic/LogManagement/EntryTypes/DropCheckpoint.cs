using System;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;

namespace maxbl4.Race.Logic.LogManagement.EntryTypes
{
    public class DropCheckpoint: IEntry
    {
        public Id<Checkpoint> TargetId { get; set; }
        public DateTime Timestamp { get; set; }
        public long Id { get; set; }
    }
}