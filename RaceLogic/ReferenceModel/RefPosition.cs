using System;
using System.Collections.Generic;
using RaceLogic.Interfaces;
using RaceLogic.Model;

namespace RaceLogic.ReferenceModel
{
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
    
    
}