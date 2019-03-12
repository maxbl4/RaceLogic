using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RaceLogic.ReferenceModel;

namespace RaceLogic.Tests.Infrastructure
{
    public class RoundDef
    {
        public List<Checkpoint<int>> Checkpoints { get; } = new List<Checkpoint<int>>();
        public List<RoundPosition> Rating { get; } = new List<RoundPosition>();
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public bool HasDuration => Duration > TimeSpan.Zero;
        
    }

    public class Cps : IEnumerable<Checkpoint<int>>
    {
        public void Add(int riderId)
        {
            this[riderId] = "";
        }
        
        public List<Checkpoint<int>> Checkpoints { get; } = new List<Checkpoint<int>>();
        public string this[int riderId]
        {
            set
            {
                Checkpoints.Add(new Checkpoint<int>
                {
                    RiderId = riderId,
                    Timestamp = DateTimeOffset.MinValue + TimeSpanExt.Parse(value)
                });
            }
        }

        public IEnumerator<Checkpoint<int>> GetEnumerator()
        {
            return Checkpoints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RoundDefParser
    {
        private const string Track = "Track";
        private const string Rating = "Rating";
        public static RoundDef ParseRoundDef(string src)
        {
            var lines = src.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var mode = Track;
            var rd = new RoundDef
            {
                Duration = ParseDuration(lines[0])
            };
            foreach (var line in lines.Skip(1))
            {
                if (line.StartsWith("#"))
                    continue;
                switch (mode)
                {
                    case Track:
                        if (line.StartsWith(Rating))
                            mode = Rating;
                        else
                        {
                            rd.Checkpoints.AddRange(ParseCheckpoints(line));
                        }
                        break;
                    case Rating:
                        rd.Rating.Add(ParseRating(line));
                        break;
                }
            }

            return rd;
        }

        private static RoundPosition ParseRating(string line)
        {
            throw new NotImplementedException();
        }

        public static TimeSpan ParseDuration(string trackHeader)
        {
            var parts = trackHeader.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                return TimeSpanExt.Parse(parts[1]);
            return TimeSpan.Zero;
        }
        
        public static IEnumerable<Checkpoint<int>> ParseCheckpoints(string line)
        {
            var stringCps = line.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stringCp in stringCps)
            {
                yield return ParseCheckpoint(stringCp);
            }
        }

        public static Checkpoint<int> ParseCheckpoint(string stringCp)
        {
            var parts = stringCp.Split(new[] {'[', ']'});
            var cp = new Checkpoint<int>();
            cp.RiderId = int.Parse(parts[0]);
            if (parts.Length > 1)
                cp.Timestamp = DateTimeOffset.MinValue + TimeSpanExt.Parse(parts[1]);
            return cp;
        }
    }
}