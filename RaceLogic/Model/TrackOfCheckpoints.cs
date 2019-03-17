using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;

namespace RaceLogic.Model
{
    public class TrackOfCheckpoints<TRiderId> where TRiderId: IEquatable<TRiderId>
    {
        readonly Dictionary<TRiderId, RoundPosition<TRiderId>> positions = new Dictionary<TRiderId, RoundPosition<TRiderId>>();
        readonly List<List<Checkpoint<TRiderId>>> track = new List<List<Checkpoint<TRiderId>>>();
        public DateTime RoundStartTime { get; }

        public TrackOfCheckpoints(DateTime? roundStartTime = null)
        {
            RoundStartTime = roundStartTime ?? default(DateTime);
        }
        
        public void Append(Checkpoint<TRiderId> cp)
        {
            var position = positions.GetOrAdd(cp.RiderId, x => RoundPosition<TRiderId>.FromStartTime(x, RoundStartTime));
            position.Append(cp);
            if (track.Count < position.LapsCount)
                track.Add(new List<Checkpoint<TRiderId>>());
            track[position.LapsCount - 1].Add(cp);
        }

        public IEnumerable<RoundPosition<TRiderId>> GetRating()
        {
            IEnumerable<RoundPosition<TRiderId>> result = null;
            for (var i = track.Count - 1; i >= 0 ; i--)
            {
                var partRating = track[i].Select(x => positions[x.RiderId]);
                result = result == null ? partRating : result.Concat(partRating);
            }

            return result;
        }
    }
}