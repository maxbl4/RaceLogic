using System;
using System.Collections.Generic;
using RaceLogic.CalculationModel;
using RaceLogic.Interfaces;

namespace RaceLogic.ReferenceModel
{
    public class Checkpoint<T> : ICheckpoint<T>
        where T: struct, IComparable, IComparable<T>, IEquatable<T>
    {
        private sealed class TimestampRelationalComparer : IComparer<Checkpoint<T>>
        {
            public int Compare(Checkpoint<T> x, Checkpoint<T> y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }

        public static IComparer<Checkpoint<T>> TimestampComparer { get; } = new TimestampRelationalComparer();

        public DateTimeOffset Timestamp { get; set; }
        public T RiderId { get; set; }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
        }
    }

    public class RefPosition : IPosition<int>
    {
        public int Points { get; set; }
        public int Position { get; set; }
        public int RiderId { get; set; }
        public bool Dsq { get; set; }
        public AggPosition<int, RefPosition> AggPosition { get; set; }
        public override string ToString()
        {
            return $"#{RiderId} {Position} {Points}";
        }
    }

    public class Lap : ILap<int, Checkpoint<int>>
    {
        public int RiderId { get; set; }
        public Checkpoint<int> Checkpoint { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan AggDuration { get; set; }
        public int Number { get; set; }
    }
    
    public class RoundPosition : IRoundPosition<int, Lap, Checkpoint<int>>
    {
        public int Points { get; set; }
        public int Position { get; set; }
        public int LapsCount { get; set; }
        public List<Lap> Laps { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Dsq { get; }
        public int RiderId { get; set; }
        
        public override string ToString()
        {
            return $"{RiderId} L:{Laps?.Count}";
        }
    }
}