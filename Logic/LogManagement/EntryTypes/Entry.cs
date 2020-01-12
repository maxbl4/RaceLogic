using System;

namespace maxbl4.Race.Logic.LogManagement.EntryTypes
{
    public interface IEntry
    {
        DateTime Timestamp { get; set; }
        long Id { get; set; }
    }
}