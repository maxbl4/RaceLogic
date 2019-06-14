using System;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class RfidCheckpoint<TRiderId> : Checkpoint<TRiderId>
        where TRiderId : IEquatable<TRiderId>
    {
        public string TagId { get; }

        public RfidCheckpoint(TRiderId riderId, DateTime timestamp, string tagId) : base(riderId, timestamp)
        {
            TagId = tagId;
        }
    }
}