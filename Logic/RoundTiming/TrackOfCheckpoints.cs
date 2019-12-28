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
        readonly HashSet<string> positionsInRating = new HashSet<string>();
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
            if (positionsInRating.Add(position.RiderId))
                Sequence.Add(position);
            Sequence.Sort(RoundPosition.LapsCountFinishedComparer);
        }
    }
}