using System;

namespace RaceLogic.Interfaces
{
    public interface ILap<TRiderId, TCheckpoint>
        where TRiderId: struct, IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
        where TCheckpoint: ICheckpoint<TRiderId>
    {
        TRiderId RiderId { get; set; }
        
        TCheckpoint Checkpoint { get; set; }
        DateTimeOffset Start { get; set; }
        DateTimeOffset End { get; set; }
        TimeSpan Duration { get; set; }
        TimeSpan AggDuration { get; set; }
        int Number { get; set; }
    }
}