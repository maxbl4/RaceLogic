using System;
using System.Collections.Generic;
using System.Threading;

namespace maxbl4.RaceLogic.LogManagement.EntryTypes
{
    public class Checkpoint: Entry
    {
        public string RiderId { get; set; }

        public override string ToString()
        {
            return $"{RiderId} Ts:{Timestamp:t}";
        }
        
        private sealed class TimestampRelationalComparer : IComparer<Checkpoint>
        {
            public int Compare(Checkpoint x, Checkpoint y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Timestamp.CompareTo(y.Timestamp);
            }
        }
        
        public static IComparer<Checkpoint> TimestampComparer { get; } = new TimestampRelationalComparer();
    }
}