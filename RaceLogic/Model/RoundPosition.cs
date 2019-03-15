using System;
using System.Collections.Generic;

namespace RaceLogic.Model
{
    public class RoundPosition<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        public int Points { get; set; }
        public int Position { get; set; }
        public int LapsCount { get; set; }
        public List<Lap<TRiderId>> Laps { get; } = new List<Lap<TRiderId>>();
        public TimeSpan Duration { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Dsq { get; }
        public TRiderId RiderId { get; set; }
        
        public override string ToString()
        {
            return $"{RiderId} L:{Laps?.Count}";
        }
    }
}