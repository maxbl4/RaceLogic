using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaceLogic.Checkpoints;
using RaceLogic.Extensions;
using RaceLogic.RoundTiming;

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
            var (start, duration) = ParseTrackHeader(lines[0]);
            var rd = new RoundDef
            {
                RoundStartTime = start,
                Duration = duration
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
                            rd.Checkpoints.AddRange(ParseCheckpoints(line, rd.RoundStartTime));
                        }
                        break;
                    case Rating:
                        rd.Rating.Add(ParseRating(line, rd.RoundStartTime));
                        break;
                }
            }

            return rd;
        }

        public static RoundPosition<int> ParseRating(string line, DateTime roundStartTime)
        {
            var parts = line.Split(new[] {' ', '\t', '[', ']', 'L', 'F'}, StringSplitOptions.RemoveEmptyEntries);
            var riderId = int.Parse(parts[0]);
            var finished = line.StartsWith('F');
            if (parts.Length == 1)
                return RoundPosition<int>.FromLaps(riderId, new Lap<int>[0], false);
            var lapCount = int.Parse(parts[1]);
            if (parts.Length - lapCount < 2)
                throw new FormatException($"Input should be in format: <rider_number> <number_of_laps> [<lap_time1> ... <lap_time_number_of_laps]. Found lapCount={lapCount} but {parts.Length-2} lap times");
                
            Lap<int> prevLap = null;
            var laps = parts.Skip(2)
                .Select((x, i) =>
                {
                    var cp = new Checkpoint<int>(riderId, roundStartTime + TimeSpanExt.Parse(x));
                    var l = prevLap?.CreateNext(cp) ?? new Lap<int>(cp, roundStartTime);
                    prevLap = l;
                    return l;
                });
            
            return RoundPosition<int>.FromLaps(riderId, laps, finished);
        }

        public static (DateTime roundStartTime, TimeSpan duration) ParseTrackHeader(string trackHeader)
        {
            var parts = trackHeader.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
                return (DateTime.MinValue, TimeSpanExt.Parse(parts[1]));
            if (parts.Length >= 3)
                return (DateTime.Parse(parts[1]), TimeSpanExt.Parse(parts[2]));
            return (DateTime.MinValue, TimeSpan.Zero);
        }
        
        public static IEnumerable<Checkpoint<int>> ParseCheckpoints(string line, DateTime roundStartTime)
        {
            var stringCps = line.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stringCp in stringCps)
            {
                yield return ParseCheckpoint(stringCp, roundStartTime);
            }
        }

        public static Checkpoint<int> ParseCheckpoint(string stringCp, DateTime roundStartTime)
        {
            var parts = stringCp.Split(new[] {'[', ']'});
            if (parts.Length > 1)
                return new Checkpoint<int>(int.Parse(parts[0]), roundStartTime + TimeSpanExt.Parse(parts[1]));
            return new Checkpoint<int>(int.Parse(parts[0]));
        }

        public static string ToDefString(this Checkpoint<int> cp, DateTime roundStartTime)
        {
            if (cp == null) return "";
            if (cp.Timestamp == default(DateTime)) return cp.RiderId.ToString();
            return $"{cp.RiderId}[{(cp.Timestamp - roundStartTime).ToShortString()}]";
        }
        
        public static string ToDefString(this RoundPosition<int> rp)
        {
            if (rp == null) return "";
            var sb = new StringBuilder();
            if (rp.Finished) sb.Append("F");
            sb.Append($"{rp.RiderId} L{rp.LapsCount}");
            if (rp.Laps.Count > 0)
            {
                sb.Append($" [{string.Join(" ", rp.Laps.Select(x => (x.End - rp.Start).ToShortString()))}]");
            }
            return sb.ToString();
        }

        public static string FormatCheckpoints(this RoundDef rd)
        {
            var sb = new StringBuilder();
            var histogram = new Dictionary<int, int>();
            var maxLaps = 0;
            foreach (var cp in rd.Checkpoints)
            {
                var laps = histogram.UpdateOrAdd(cp.RiderId, x => x + 1);
                if (laps > maxLaps)
                {
                    sb.AppendLine();
                    maxLaps = laps;
                }
                else
                    sb.Append(' ');
                
                sb.Append(cp.ToDefString(rd.RoundStartTime));
            }

            return sb.ToString();
        }
    }
}