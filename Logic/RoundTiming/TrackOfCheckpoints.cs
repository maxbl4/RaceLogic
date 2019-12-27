using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class TrackOfCheckpoints
    {
        private bool finishForced;
        private readonly IFinishCriteria finishCriteria;
        readonly Dictionary<string, RoundPosition> positions = new Dictionary<string, RoundPosition>();
        readonly List<List<Checkpoint>> track = new List<List<Checkpoint>>();
        public DateTime RoundStartTime { get; }

        public TrackOfCheckpoints(DateTime? roundStartTime = null, IFinishCriteria finishCriteria = null)
        {
            this.finishCriteria = finishCriteria;
            RoundStartTime = roundStartTime ?? default;
        }
        
        public void Append(Checkpoint cp)
        {
            if (finishForced) return;
            var position = positions.GetOrAdd(cp.RiderId, x => RoundPosition.FromStartTime(x, RoundStartTime));
            if (position.Finished)
                return;
            position.Append(cp);
            if (track.Count < position.LapsCount)
                track.Add(new List<Checkpoint>());
            track[position.LapsCount - 1].Add(cp);
            UpdateSequence(position);
            if (finishCriteria?.HasFinished(position, Sequence, false) == true)
            {
                position.Finish();
                UpdateSequence(position);
            }
        }

        public void ForceFinish()
        {
            foreach (var position in Sequence)
            {
                if (finishCriteria?.HasFinished(position, Sequence, true) == true)
                    position.Finish();
            }
            Sequence.Sort(RoundPosition.LapsCountFinishedComparer);
            finishForced = true;
        }

        public List<RoundPosition> Sequence { get; } = new List<RoundPosition>();

        private void UpdateSequence(RoundPosition position)
        {
            if (Sequence.All(x => x.RiderId != position.RiderId))
                Sequence.Add(position);
            Sequence.Sort(RoundPosition.LapsCountFinishedComparer);
        }

        private IEnumerable<RoundPosition> GetSequenceOld()
        {
            IEnumerable<RoundPosition> result = null;
            for (var i = track.Count - 1; i >= 0 ; i--)
            {
                var lapIndex = i + 1;
                var partRating = track[i].Select(x => positions[x.RiderId]).Where(x => x.LapsCount == lapIndex);
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