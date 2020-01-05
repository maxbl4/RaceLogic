using System;
using System.Collections.Generic;
using System.Text;
using Cli.BraaapWebModel;
using maxbl4.Infrastructure.Extensions.DictionaryExt;

namespace Cli.Serialization
{
    public static class RoundDefParser
    {
        public const string Track = "Track";
        public const string Rating = "Rating";
        
        public static string ToDefString(this Checkpoint cp, DateTime roundStartTime)
        {
            if (cp == null) return "";
            if (cp.Timestamp == default) return cp.Number.ToString();
            return $"{cp.Number}[{(cp.Timestamp - roundStartTime).ToShortString()}]";
        }
       
        
        public static string FormatCheckpoints(IEnumerable<Checkpoint> checkpoints, DateTime roundStartTime)
        {
            var sb = new StringBuilder();
            var histogram = new Dictionary<string, int>();
            var maxLaps = 0;
            foreach (var cp in checkpoints)
            {
                var laps = histogram.UpdateOrAdd(cp.Number.ToString(), x => x + 1);
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