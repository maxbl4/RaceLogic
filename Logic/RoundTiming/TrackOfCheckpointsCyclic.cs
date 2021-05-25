using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class TrackOfCheckpointsCyclic : ITrackOfCheckpoints
    {
        private readonly Dictionary<string, RoundPosition> positions = new();
        private bool finishForced;

        private List<RoundPosition> rating;

        public TrackOfCheckpointsCyclic(DateTime? roundStartTime = null, IFinishCriteria finishCriteria = null)
        {
            FinishCriteria = finishCriteria;
            RoundStartTime = roundStartTime ?? default;
        }

        public IFinishCriteria FinishCriteria { get; }
        public List<List<Checkpoint>> Track { get; } = new();
        public List<Checkpoint> Checkpoints { get; } = new();
        public DateTime RoundStartTime { get; }

        public void Append(Checkpoint cp)
        {
            Checkpoints.Add(cp);
            if (finishForced) return;
            var position = positions.GetOrAdd(cp.RiderId, x => RoundPosition.FromStartTime(x, RoundStartTime));
            if (position.Finished)
                return;
            position.Append(cp);
            if (Track.Count < position.LapCount)
                Track.Add(new List<Checkpoint>());
            Track[position.LapCount - 1].Add(cp);
            if (FinishCriteria?.HasFinished(position, GetSequence(), false) == true) position.Finish();
            rating = null;
        }

        public void ForceFinish()
        {
            foreach (var position in GetSequence())
                if (FinishCriteria?.HasFinished(position, GetSequence(), true) == true)
                    position.Finish();
            finishForced = true;

            rating = null;
        }

        public List<RoundPosition> Rating => rating ??= GetSequence().ToList();

        private IEnumerable<RoundPosition> GetSequence()
        {
            IEnumerable<RoundPosition> result = null;
            for (var i = Track.Count - 1; i >= 0; i--)
            {
                var lapIndex = i + 1;
                var partRating = Track[i].Select(x => positions[x.RiderId]).Where(x => x.LapCount == lapIndex);
                result = result == null ? partRating : result.Concat(partRating);
            }

            if (result == null)
                return new RoundPosition[0];

            return result.Select((x, i) => new
                    {Position = x, Index = i, IndexOfStartAndFinish = x.IndexOfStartAndFinish()})
                .OrderByDescending(x => x.IndexOfStartAndFinish)
                .ThenBy(x => x.Index)
                .Select(x => x.Position);
        }
    }
}