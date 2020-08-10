using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
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

        public static RoundPosition ParseRating(string line, DateTime roundStartTime)
        {
            var parts = line.Split(new[] {' ', '\t', '[', ']', 'L', 'F'}, StringSplitOptions.RemoveEmptyEntries);
            var riderId = parts[0];
            var finished = line.StartsWith('F');
            if (parts.Length == 1)
                return RoundPosition.FromLaps(riderId, new Lap[0], false);
            var lapCount = int.Parse(parts[1]);
            if (parts.Length - lapCount < 2)
                throw new FormatException($"Input should be in format: <rider_number> <number_of_laps> [<lap_time1> ... <lap_time_number_of_laps]. Found lapCount={lapCount} but {parts.Length-2} lap times");
                
            Lap prevLap = null;
            var laps = parts.Skip(2)
                .Select((x, i) =>
                {
                    var cp = new Checkpoint(riderId, roundStartTime + TimeSpanExt.Parse(x));
                    var l = prevLap?.CreateNext(cp) ?? new Lap(cp, roundStartTime);
                    prevLap = l;
                    return l;
                });
            
            return RoundPosition.FromLaps(riderId, laps, finished);
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
        
        public static IEnumerable<Checkpoint> ParseCheckpoints(string line, DateTime roundStartTime)
        {
            var stringCps = line.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stringCp in stringCps)
            {
                yield return ParseCheckpoint(stringCp, roundStartTime);
            }
        }

        public static Checkpoint ParseCheckpoint(string stringCp, DateTime roundStartTime)
        {
            var parts = stringCp.Split('[', ']');
            if (parts.Length > 1)
                return new Checkpoint(parts[0], roundStartTime + TimeSpanExt.Parse(parts[1]));
            return new Checkpoint(parts[0], Constants.DefaultUtcDate);
        }

        public static string ToDefString(this Checkpoint cp, DateTime roundStartTime)
        {
            if (cp == null) return "";
            if (cp.Timestamp == default) return cp.RiderId;
            return $"{cp.RiderId}[{(cp.Timestamp - roundStartTime).ToShortString()}]";
        }
        
        public static string ToDefString(this RoundPosition rp)
        {
            if (rp == null) return "";
            var sb = new StringBuilder();
            if (rp.Finished) sb.Append("F");
            sb.Append($"{rp.RiderId} {rp.LapCount}");
            if (rp.Laps.Count > 0)
            {
                sb.Append($" [{string.Join(" ", rp.Laps.Select(x => (x.End - rp.Start).ToShortString()))}]");
            }
            return sb.ToString();
        }

        public static string FormatCheckpoints(this RoundDef rd)
        {
            return FormatCheckpoints(rd.Checkpoints, rd.RoundStartTime);
        }
        
        public static string FormatCheckpoints(IEnumerable<Checkpoint> checkpoints, DateTime roundStartTime)
        {
            var sb = new StringBuilder();
            var histogram = new Dictionary<string, int>();
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
                
                sb.Append(ToDefString(cp, roundStartTime));
            }

            return sb.ToString();
        }
    }
}