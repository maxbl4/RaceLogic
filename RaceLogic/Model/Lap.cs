using System;
using RaceLogic.Checkpoints;
using RaceLogic.Interfaces;

namespace RaceLogic.Model
{
    public class Lap<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public Checkpoint<TRiderId> Checkpoint { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration { get; }
        public TimeSpan AggDuration { get; }
        public int SequentialNumber { get; }

        public Lap(Checkpoint<TRiderId> checkpoint, DateTime roundStartTime)
        {
            Checkpoint = checkpoint;
            Start = checkpoint.HasTimestamp ? roundStartTime: default(DateTime);
            End = checkpoint.Timestamp;
            Duration = AggDuration = End - Start;
            SequentialNumber = 1;
        }
        
        public Lap(Checkpoint<TRiderId> checkpoint, Lap<TRiderId> previousLap)
        {
            Checkpoint = checkpoint;
            Start = previousLap.End;
            End = checkpoint.Timestamp;
            Duration = End - Start;
            AggDuration = previousLap.AggDuration + Duration;
            SequentialNumber = previousLap.SequentialNumber + 1;
        }

        public Lap<TRiderId> CreateNext(Checkpoint<TRiderId> checkpoint)
        {
            return new Lap<TRiderId>(checkpoint, this);
        }
    }
}