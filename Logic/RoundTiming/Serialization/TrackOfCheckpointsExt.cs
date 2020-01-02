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
            return sb.ToString();
        }
    }
}