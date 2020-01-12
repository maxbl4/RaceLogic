using System;
using maxbl4.Race.Logic.LogManagement.EntryTypes;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ICheckpoint : IEntry
    {
        DateTime Timestamp { get; set; }
        string RiderId { get; set; }
        long Id { get; set; }
    }
}