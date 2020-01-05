using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.Infrastructure.Extensions.DictionaryExt;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming
{
    public class TrackOfCheckpointsIncrementalCustomSort : ITrackOfCheckpoints
    {
        private bool finishForced;
        public IFinishCriteria FinishCriteria { get; }
        readonly Dictionary<string, RoundPosition> positions = new Dictionary<string, RoundPosition>();
        public List<List<Checkpoint>> Track { get; } = new List<List<Checkpoint>>();
        public List<Checkpoint> Checkpoints { get; } = new List<Checkpoint>();
        public DateTime RoundStartTime { get; }

        public TrackOfCheckpointsIncrementalCustomSort(DateTime? roundStartTime = null, IFinishCriteria finishCriteria = null)
        {
            this.FinishCriteria = finishCriteria;
            RoundStartTime = roundStartTime ?? default;
        }
        
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
            UpdateSequence(position);
            if (FinishCriteria?.HasFinished(position, Rating, false) == true)
            {
                position.Finish();
                UpdateSequence(position);
            }
        }

        public void ForceFinish()
        {
            foreach (var position in Rating.Where(x => !x.Finished).ToList())
            {
                if (FinishCriteria?.HasFinished(position, Rating, true) == true)
                {
                    position.Finish();
                    UpdateSequence(position);
                }
            }
            finishForced = true;
        }

        public List<RoundPosition> Rating { get; } = new List<RoundPosition>();

        private void UpdateSequence(RoundPosition position)
        {
            // if (positionsInRating.TryGetValue(position.RiderId, out var currentIndex))
            var currentIndex = Rating.FindIndex(x => x.RiderId == position.RiderId);
            if (currentIndex >= 0)
                Rating.RemoveAt(currentIndex);
            var newIndex = Rating.Count - 1;
            while (newIndex >= 0 && RoundPosition.LapsCountFinishedComparer.Compare(Rating[newIndex], position) > 0)
                newIndex--;
            newIndex++;
            Rating.Insert(newIndex, position);
        }
    }
}