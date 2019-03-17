using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;
using RaceLogic.Model;

namespace RaceLogic
{
    public class ClassicRoundResultStrategyResult<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        public bool EveryoneFinished { get; set; }
        public DateTime StopTime { get; set; }
        public List<RoundPosition<TRiderId>> Rating { get; set; } = new List<RoundPosition<TRiderId>>();
        public List<Checkpoint<TRiderId>> Checkpoints { get; set; }
    }

    public class ClassicRoundResultStrategy<TRiderId>
        where TRiderId: IEquatable<TRiderId>
    {
        private readonly Settings settings;
        public ClassicRoundResultStrategy(Settings settings = null)
        {
            this.settings = settings ?? Settings.Default;
        }

        public ClassicRoundResultStrategyResult<TRiderId> Process(List<Checkpoint<TRiderId>> checkpoints, HashSet<TRiderId> riderIds, 
            DateTime roundStartTime, TimeSpan roundDuration, bool forcedFinish)
        {
            var result = new ClassicRoundResultStrategyResult<TRiderId>();
            result.Checkpoints = checkpoints;
            HashSet<TRiderId> didNotFinish = null;
            if (forcedFinish)
            {
                var finishTime = roundStartTime + roundDuration;
                didNotFinish = new HashSet<TRiderId>(checkpoints.GroupBy(x => x.RiderId)
                    .Where(x => x.All(cp => cp.Timestamp < finishTime))
                    .Select(x => x.Key));
            }

            var records = new Dictionary<TRiderId, RoundPosition<TRiderId>>();
            var leaderHasFinished = false;
            var maxLaps = 0;
            var leaderDuration = TimeSpan.MaxValue;
            foreach (var cp in checkpoints)
            {
                var rec = records.GetOrAdd(cp.RiderId, n => RoundPosition<TRiderId>.FromStartTime(cp.RiderId, roundStartTime));
                //TODO: onNewPosition(rec);
                if (rec.Finished)
                {
                    continue;
                }
                //TODO: onNewLap(lap);
                records[cp.RiderId] = rec = rec.Append(cp);
                if (!forcedFinish || !didNotFinish.Contains(cp.RiderId))
                {
                    if (maxLaps < rec.LapsCount)
                    {
                        maxLaps = Math.Max(maxLaps, rec.LapsCount);
                        leaderDuration = rec.Duration;
                    }

                    if (maxLaps == rec.LapsCount && leaderDuration > rec.Duration)
                        leaderDuration = rec.Duration;
                    if (maxLaps == rec.LapsCount && rec.Duration > roundDuration && rec.Duration <= leaderDuration)
                        leaderHasFinished = true;
                    if (leaderHasFinished && rec.Duration >= leaderDuration)
                        records[cp.RiderId] = rec = rec.Finish();
                }
            }
            var ridersWithoutLaps = riderIds.Except(records.Keys);
            if (records.Any() && records.All(x => x.Value.Finished))
            {
                result.EveryoneFinished = true;
                result.StopTime = checkpoints.Last().Timestamp;
            }

            var maxPoints = records.Values.Count(x => x.Finished);
            
            result.Rating = records.Values
                .Concat(ridersWithoutLaps.Select(x => RoundPosition<TRiderId>.FromStartTime(x, roundStartTime))) //TODO: onNewPosition()
                .OrderByDescending(x => x.Finished ? 1 : 0)
                .ThenByDescending(x => x.LapsCount)
                .ThenBy(x => x.Duration)
                .ThenBy(x => x.RiderId)
                .Select((x, i) => {
                                x.Position = i + 1;
                                x.Points = Math.Max(0, maxPoints - i);
                            return x;
                        })
                .ToList();
            return result;
        }

        public class Settings
        {
            public static Settings Default { get; } = new Settings();
            /// <summary>
            /// In case the start is going through the gate, you may prefer to ignore first lap reading
            /// </summary>
            public bool SkipFirstLap { get; set; }
            /// <summary>
            /// For 15 minutes +1 lap logic. Set how many laps you want
            /// </summary>
            public int PlusLapsAfterRoundTime { get; set; }
//            public Func<TPosition, TPosition> OnNewPosition { get; set; }
//            public Func<TLap, TLap> OnNewLap { get; set; }
        }
    }
}