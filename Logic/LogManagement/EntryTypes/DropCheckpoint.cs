using System;

namespace maxbl4.Race.Logic.LogManagement.EntryTypes
{
    public class DropCheckpoint: IEntry
    {
        public long TargetId { get; set; }
        public DateTime Timestamp { get; set; }
        public long Id { get; set; }
    }
}