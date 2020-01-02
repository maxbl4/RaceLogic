using System.Linq;
using System.Text;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
{
    public static class TrackOfCheckpointsExt
    {
        public static string ToRoundDefString(this ITrackOfCheckpoints track)
        {
            //var hasTime = track.RoundStartTime > Constants.DefaultUtcDate;
            var sb = new StringBuilder();
            sb.AppendLine($"Track {track.FinishCriteria.Duration.ToShortString()}");
            foreach (var line in track.Track)
            {
                sb.AppendLine(string.Join(" ", line.Select(checkpoint =>
                {
                    if (checkpoint.Timestamp > Constants.DefaultUtcDate)
                        return $"{checkpoint.RiderId}[{(checkpoint.Timestamp - track.RoundStartTime).ToShortString()}]";
                    return $"{checkpoint.RiderId}";
                })));
            }

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