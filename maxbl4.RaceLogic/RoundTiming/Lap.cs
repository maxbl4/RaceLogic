using System;
using maxbl4.RaceLogic.Checkpoints;

namespace maxbl4.RaceLogic.RoundTiming
{
    public class Lap
    {
        public Checkpoint Checkpoint { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public TimeSpan Duration { get; }
        public TimeSpan AggDuration { get; }
        public int SequentialNumber { get; }

        public Lap(Checkpoint checkpoint, DateTime roundStartTime)
        {
            Checkpoint = checkpoint;
            Start = roundStartTime;
            End = checkpoint.Timestamp;
            Duration = AggDuration = End - Start;
            SequentialNumber = 1;
        }
        
        public Lap(Checkpoint checkpoint, Lap previousLap)
        {
            Checkpoint = checkpoint;
            Start = previousLap.End;
            End = checkpoint.Timestamp;
            Duration = End - Start;
            AggDuration = previousLap.AggDuration + Duration;
            SequentialNumber = previousLap.SequentialNumber + 1;
        }

        public Lap CreateNext(Checkpoint checkpoint)
        {
            return new Lap(checkpoint, this);
        }
    }
}