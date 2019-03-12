using System;
using System.Collections.Generic;

namespace RaceLogic.Interfaces
{
    public interface IRoundPosition<TRiderId, TLap, TCheckpoint> : IPosition<TRiderId>
        where TRiderId : struct, IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
        where TLap: ILap<TRiderId, TCheckpoint>
        where TCheckpoint: ICheckpoint<TRiderId>
    {
        new TRiderId RiderId { get; set; }
        new int Points { get; set; }
        new int Position { get; set; }
        
        int LapsCount { get; set; }
        List<TLap> Laps { get; set; }
        TimeSpan Duration { get; set; }
        DateTimeOffset Start { get; set; }
        DateTimeOffset End { get; set; }
        bool Finished { get; set; }
        bool Started { get; set; }
    }
}