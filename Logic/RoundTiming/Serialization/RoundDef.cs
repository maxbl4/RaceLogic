using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
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
            sb.Append(string.Join(Environment.NewLine, Rating.Select(x => RoundDefParser.ToDefString(x))));
            return sb.ToString();
        }
    }
}