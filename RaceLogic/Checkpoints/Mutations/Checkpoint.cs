using System;

namespace RaceLogic.Checkpoints.Mutations
{
    public static class Checkpoint
    {
        public static Checkpoint<TRiderId> WithRiderId<TRiderId>(this Checkpoint<TRiderId> cp, TRiderId newRiderId)
            where TRiderId: IEquatable<TRiderId>
        {
            return cp == null ? null : new Checkpoint<TRiderId>(newRiderId, cp.Timestamp);
        }
        
        public static Checkpoint<TRiderId> WithTimestamp<TRiderId>(this Checkpoint<TRiderId> cp, DateTime newTimestamp)
            where TRiderId: IEquatable<TRiderId>
        {
            return cp == null ? null : new Checkpoint<TRiderId>(cp.RiderId, newTimestamp);
        }
    }
}