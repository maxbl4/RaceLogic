using System;
using maxbl4.Race.Logic.LogManagement.EntryTypes;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ICheckpoint
    {
        DateTime Timestamp { get; set; }
        string RiderId { get; set; }
        DateTime LastSeen { get; set; }
        int Count { get; set; }
        bool Aggregated { get; set; }
        bool IsManual { get; set; }
        int Rps { get; set; }
    }
}