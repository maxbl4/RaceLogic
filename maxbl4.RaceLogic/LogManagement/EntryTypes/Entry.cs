using System;

namespace maxbl4.RaceLogic.LogManagement.EntryTypes
{
    public class Entry
    {
        public DateTime Timestamp { get; set; }
        public bool HasTimestamp => Timestamp > default(DateTime);
        public long Sequence { get; set; }
    }
}