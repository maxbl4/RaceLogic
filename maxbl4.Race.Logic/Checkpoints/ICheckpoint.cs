using System;

namespace maxbl4.Race.Logic.Checkpoints
{
    public interface ICheckpoint
    {
        DateTime Timestamp { get; set; }
        string RiderId { get; set; }
        long Id { get; set; }
    }
}