using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.RoundTiming;

namespace maxbl4.RaceLogic.Tests.Infrastructure
{
    public class RoundDef
    {
        public List<Checkpoint> Checkpoints { get; } = new List<Checkpoint>();
        public List<RoundPosition> Rating { get; } = new List<RoundPosition>();
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
            sb.Append(string.Join(Environment.NewLine, Rating.Select(x => x.ToDefString())));
            return sb.ToString();
        }
    }
}