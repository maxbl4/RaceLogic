using System;
using System.Linq;
using System.Text;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
{
    public static class TrackOfCheckpointsExt
    {
        public static string ToRoundDefString(this ITrackOfCheckpoints track)
        {
            var sb = new StringBuilder();
            sb.Append($"Track");
            if (track.FinishCriteria.Duration > TimeSpan.Zero)
                sb.Append($" {track.FinishCriteria.Duration.ToShortString()}");
            sb.AppendLine(RoundDefParser.FormatCheckpoints(track.Checkpoints, track.RoundStartTime));
            sb.AppendLine("Rating");
            for (var i = 0; i < track.Rating.Count; i++)
            {
                var position = track.Rating[i];
                if (position.Finished) sb.Append("F");
                sb.Append(position.RiderId);
                sb.Append($" {position.LapCount} ");
                sb.Append($"[{string.Join(" ", position.Laps.Select(l => l.AggDuration.ToShortString()))}]");
                if (track.Rating.Count - i > 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}