using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaceLogic.Extensions;
using RaceLogic.Model;
using RaceLogic.ReferenceModel;

namespace RaceLogic.Tests.Infrastructure
{
    public static class RoundDefParser
    {
        public const string Track = "Track";
        public const string Rating = "Rating";
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

        private static RoundPosition<int> ParseRating(string line)
        {
            var parts = line.Split(new[] {' ', '\t', '[', ']', 'L', 'F'}, StringSplitOptions.RemoveEmptyEntries);
            var rp = new RoundPosition<int>
            {
                RiderId = int.Parse(parts[0]),
                LapsCount = int.Parse(parts[1]),
                Finished = line.StartsWith('F')
            };

            Lap<int> prevLap = null;
            rp.Laps.AddRange(parts.Skip(2)
                .Select((x, i) =>
                {
                    var cp = new Checkpoint<int>(rp.RiderId, DateTime.MinValue + TimeSpanExt.Parse(x));
                    var l = prevLap?.CreateNext(cp) ?? new Lap<int>(cp, DateTime.MinValue);
                    prevLap = l;
                    return l;
                }));
            if (rp.Laps.Count > 0)
                rp.LapsCount = rp.Laps.Count; 
            return rp;
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
            if (parts.Length > 1)
                return new Checkpoint<int>(int.Parse(parts[0]), DateTime.MinValue + TimeSpanExt.Parse(parts[1]));
            return new Checkpoint<int>(int.Parse(parts[0]));
        }

        public static string ToDefString(this Checkpoint<int> cp)
        {
            if (cp == null) return "";
            if (cp.Timestamp == DateTime.MinValue) return cp.RiderId.ToString();
            return $"{cp.RiderId}[{(cp.Timestamp - DateTime.MinValue).ToShortString()}]";
        }
        
        public static string ToDefString(this RoundPosition<int> rp)
        {
            if (rp == null) return "";
            var sb = new StringBuilder();
            if (rp.Finished) sb.Append("F");
            sb.Append($"{rp.RiderId} L{rp.LapsCount}");
            if (rp.Laps.Count > 0)
            {
                sb.Append($" [{string.Join(" ", rp.Laps.Select(x => (x.End - DateTime.MinValue).ToShortString()))}]");
            }
            return sb.ToString();
        }

        public static string FormatCheckpoints(this IEnumerable<Checkpoint<int>> checkpoints)
        {
            var sb = new StringBuilder();
            var histogram = new Dictionary<int, int>();
            var maxLaps = 0;
            foreach (var cp in checkpoints)
            {
                var laps = histogram.UpdateOrAdd(cp.RiderId, x => x + 1);
                if (laps > maxLaps)
                {
                    sb.AppendLine();
                    maxLaps = laps;
                }
                else
                    sb.Append(' ');
                
                sb.Append(cp.ToDefString());
            }

            return sb.ToString();
        }
    }
}