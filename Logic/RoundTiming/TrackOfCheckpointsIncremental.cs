using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class TrackOfCheckpointsIncremental : ITrackOfCheckpoints
    {
        private bool finishForced;
        public IFinishCriteria FinishCriteria { get; }
        readonly Dictionary<string, RoundPosition> positions = new Dictionary<string, RoundPosition>();
        public List<List<Checkpoint>> Track { get; } = new List<List<Checkpoint>>();
        public DateTime RoundStartTime { get; }

        public TrackOfCheckpointsIncremental(DateTime? roundStartTime = null, IFinishCriteria finishCriteria = null)
        {
            this.FinishCriteria = finishCriteria;
            RoundStartTime = roundStartTime ?? default;
        }
        
        public void Append(Checkpoint cp)
        {
            if (finishForced) return;
            var position = positions.GetOrAdd(cp.RiderId, x => RoundPosition.FromStartTime(x, RoundStartTime));
            if (position.Finished)
                return;
            position.Append(cp);
            if (Track.Count < position.LapsCount)
                Track.Add(new List<Checkpoint>());
            Track[position.LapsCount - 1].Add(cp);
            UpdateSequence(position);
            if (FinishCriteria?.HasFinished(position, Rating, false) == true)
            {
                position.Finish();
                UpdateSequence(position);
            }
        }

        public void ForceFinish()
        {
            foreach (var position in Rating)
            {
                if (FinishCriteria?.HasFinished(position, Rating, true) == true)
                    position.Finish();
            }
            Rating.Sort(RoundPosition.LapsCountFinishedComparer);
            finishForced = true;
        }

        public List<RoundPosition> Rating { get; } = new List<RoundPosition>();

        private void UpdateSequence(RoundPosition position)
        {
            if (Rating.All(x => x.RiderId != position.RiderId))
                Rating.Add(position);
            Rating.Sort(RoundPosition.LapsCountFinishedComparer);
        }

        private IEnumerable<RoundPosition> GetSequenceOld()
        {
            IEnumerable<RoundPosition> result = null;
            for (var i = Track.Count - 1; i >= 0 ; i--)
            {
                var lapIndex = i + 1;
                var partRating = Track[i].Select(x => positions[x.RiderId]).Where(x => x.LapsCount == lapIndex);
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