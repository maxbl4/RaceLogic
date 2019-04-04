using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaceLogic.Checkpoints;
using RaceLogic.RoundTiming;

namespace RaceLogic.Tests.Infrastructure
{
    public class RoundDef
    {
        public List<Checkpoint<int>> Checkpoints { get; } = new List<Checkpoint<int>>();
        public List<RoundPosition<int>> Rating { get; } = new List<RoundPosition<int>>();
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public bool HasDuration => Duration > TimeSpan.Zero;
        public DateTime RoundStartTime { get; set; }

        public static RoundDef Parse(string src)
        {
            return RoundDefParser.ParseRoundDef(src);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(RoundDefParser.Track);
            if (HasDuration)
                sb.Append(" " + Duration.ToShortString());
            sb.AppendLine(this.FormatCheckpoints());
            sb.AppendLine(RoundDefParser.Rating);
            sb.Append(string.Join("\r\n", Rating.Select(x => x.ToDefString())));
            return sb.ToString();
        }
    }
}