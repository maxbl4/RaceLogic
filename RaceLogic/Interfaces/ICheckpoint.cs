using System;

namespace RaceLogic.Interfaces
{
    public interface ICheckpoint<out TRiderId>
        where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        DateTime Timestamp { get; }
        TRiderId RiderId { get; }
    }
    
    public interface IAggCheckpoint<out TRiderId> : ICheckpoint<TRiderId>
        where TRiderId: IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
    {
        int Number { get; set; }
        string TagId { get; set; }
        int TotalCount { get; set; }
        int ManualCount { get; set; }
        int RfidCount { get; set; }
        DateTime LastSeen { get; set; }
    }
}