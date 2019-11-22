using System;

namespace maxbl4.RaceLogic.Checkpoints
{
    public interface ICheckpoint
    {
        DateTime Timestamp { get; set; }
        string RiderId { get; set; }
        long Id { get; set; }
    }
}