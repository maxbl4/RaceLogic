using System;

namespace RaceLogic.Interfaces
{
    public interface IPosition<out TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        int Points { get; }
        int Position { get; }
        bool Dsq { get; }
        TRiderId RiderId { get; }
    }
}