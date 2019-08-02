using System;

namespace maxbl4.RaceLogic.Checkpoints
{
    public class RfidCheckpoint : Checkpoint
    {
        public string TagId { get; }

        public RfidCheckpoint(string riderId, DateTime timestamp, string tagId) : base(riderId, timestamp)
        {
            TagId = tagId;
        }
    }
}