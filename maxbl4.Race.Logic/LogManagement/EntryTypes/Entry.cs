using System;

namespace maxbl4.Race.Logic.LogManagement.EntryTypes
{
    public class Entry
    {
        public DateTime Timestamp { get; set; }
        public bool HasTimestamp => Timestamp > default(DateTime);
        public long Id { get; set; }
    }
}