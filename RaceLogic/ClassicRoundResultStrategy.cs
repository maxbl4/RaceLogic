using System;
using System.Collections.Generic;
using System.Linq;
using RaceLogic.Extensions;
using RaceLogic.Interfaces;

namespace RaceLogic
{
    public class ClassicRoundResultStrategyResult<TRiderId, TCheckpoint, TPosition, TLap>
        where TRiderId: struct, IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
        where TCheckpoint: ICheckpoint<TRiderId>
        where TPosition: IRoundPosition<TRiderId, TLap, TCheckpoint>, new()
        where TLap: ILap<TRiderId, TCheckpoint>, new()
    {
        public bool EveryoneFinished { get; set; }
        public DateTimeOffset StopTime { get; set; }
        public List<TPosition> Rating { get; set; } = new List<TPosition>();
        public List<TCheckpoint> Checkpoints { get; set; }
    }

    public class ClassicRoundResultStrategy<TRiderId, TCheckpoint, TPosition, TLap>
        where TRiderId: struct, IComparable, IComparable<TRiderId>, IEquatable<TRiderId>
        where TCheckpoint: ICheckpoint<TRiderId>
        where TPosition: IRoundPosition<TRiderId, TLap, TCheckpoint>, new()
        where TLap: ILap<TRiderId, TCheckpoint>, new()
    {
        private readonly Settings settings;
        private readonly Func<TPosition, TPosition> onNewPosition;
        private readonly Func<TLap, TLap> onNewLap;

        public ClassicRoundResultStrategy(Settings settings = null)
        {
            this.settings = settings ?? Settings.Default;
            this.onNewPosition = this.settings.OnNewPosition ?? (x => x);
            this.onNewLap = this.settings.OnNewLap ?? (x => x);
        }

        public ClassicRoundResultStrategyResult<TRiderId, TCheckpoint, TPosition, TLap> Process(List<TCheckpoint> checkpoints, HashSet<TRiderId> riderIds, 
            DateTimeOffset roundStartTime, TimeSpan roundDuration)
        {
            var result = new ClassicRoundResultStrategyResult<TRiderId, TCheckpoint, TPosition, TLap>();
            result.Checkpoints = checkpoints;
            //if (checkpoints.Count == 0) return result;
            var records = new Dictionary<TRiderId, TPosition>();
            var leaderHasFinished = false;
            var maxLaps = 0;
            var leaderDuration = TimeSpan.MaxValue;
            foreach (var cp in checkpoints)
            {
                //if (!riderIds.Contains(cp.RiderId)) continue;
                var rec = records.GetOrAdd(cp.RiderId, n => new TPosition {
                    RiderId = cp.RiderId,
                    Laps = new List<TLap>(),
                    Start = roundStartTime,
                    Started = true
                });
                rec = onNewPosition(rec);
                if (rec.Finished)
                {
                    continue;
                }
                var lastTimestamp = rec.Laps.LastOrDefault()?.End ?? roundStartTime;
                var lap = new TLap
                          {
                              Duration = cp.Timestamp - lastTimestamp,
                              AggDuration = cp.Timestamp - roundStartTime,
                              Start = lastTimestamp,
                              Checkpoint = cp,
                              RiderId = cp.RiderId,
                              End = cp.Timestamp,
                              Number = rec.LapsCount + 1
                          };
                lap = onNewLap(lap);
                rec.Duration = lap.AggDuration;
                rec.End = cp.Timestamp;
                rec.Laps.Add(lap);
                rec.LapsCount = rec.Laps.Count;
                if (maxLaps < rec.LapsCount)
                {
                    maxLaps = Math.Max(maxLaps, rec.LapsCount);
                    leaderDuration = rec.Duration;
                }
                if (maxLaps == rec.LapsCount && leaderDuration > rec.Duration)
                    leaderDuration = rec.Duration;
                if (maxLaps == rec.LapsCount && rec.Duration > roundDuration && rec.Duration <= leaderDuration && !leaderHasFinished)
                    leaderHasFinished = true;
                if (leaderHasFinished && rec.Duration >= leaderDuration)
                    rec.Finished = true;
            }
            var ridersWithoutLaps = riderIds.Except(records.Keys);
            if (records.Any() && records.All(x => x.Value.Finished))
            {
                result.EveryoneFinished = true;
                result.StopTime = checkpoints.Last().Timestamp;
            }

            var maxPoints = records.Values.Count(x => x.Finished);
            
            result.Rating = records.Values
                .Concat(ridersWithoutLaps.Select(x => onNewPosition(new TPosition
                    {
                        RiderId = x,
                        Laps = new List<TLap>(),
                    })))
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
            public Func<TPosition, TPosition> OnNewPosition { get; set; }
            public Func<TLap, TLap> OnNewLap { get; set; }
        }
    }
}