using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.Extensions;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class TrackOfCheckpoints : ITrackOfCheckpoints
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
            // if (positionsInRating.TryGetValue(position.RiderId, out var currentIndex))
            var currentIndex = Sequence.FindIndex(x => x.RiderId == position.RiderId);
            if (currentIndex >= 0)
                Sequence.RemoveAt(currentIndex);
            var newIndex = Sequence.Count - 1;
            while (newIndex >= 0 && RoundPosition.LapsCountFinishedComparer.Compare(Sequence[newIndex], position) > 0)
                newIndex--;
            newIndex++;
            Sequence.Insert(newIndex, position);
        }
    }
}