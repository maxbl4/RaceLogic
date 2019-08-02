using System;
using System.Collections.Generic;
using System.Linq;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Extensions;

namespace maxbl4.RaceLogic.RoundTiming
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
            RoundStartTime = roundStartTime ?? default(DateTime);
        }
        
        public void Append(Checkpoint cp)
        {
            if (finishForced) return;
            var position = positions.GetOrAdd(cp.RiderId, x => RoundPosition.FromStartTime(x, RoundStartTime));
            if (position.Finished)
                return;
            positions[cp.RiderId] = position = position.Append(cp);
            if (track.Count < position.LapsCount)
                track.Add(new List<Checkpoint>());
            track[position.LapsCount - 1].Add(cp);
            if (finishCriteria?.HasFinished(position, GetSequence(), false) == true)
            {
                positions[cp.RiderId] = position.Finish();
            }
            sequence = null;
        }

        public void ForceFinish()
        {
            foreach (var position in GetSequence())
            {
                if (finishCriteria?.HasFinished(position, GetSequence(), true) == true)
                    positions[position.RiderId] = position.Finish();
            }
            finishForced = true;

            sequence = null;
        }

        private List<RoundPosition> sequence = null;
        public List<RoundPosition> Sequence => sequence ?? (sequence = GetSequence().ToList());

        public IEnumerable<RoundPosition> GetSequence()
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