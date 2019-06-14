using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Extensions;

namespace maxbl4.RaceLogic
{
    public class RelayRaceRoundResultStrategy<TRiderId, TTeamId> : IObserver<Checkpoint<TRiderId>>,
        IObservable<Checkpoint<TTeamId>> 
        where TRiderId: IEquatable<TRiderId>
        where TTeamId: IEquatable<TTeamId>
    {
        private readonly List<TTeamId> emptyTeamIds = new List<TTeamId>();
        private readonly Subject<Checkpoint<TTeamId>> teamCheckpoints = new Subject<Checkpoint<TTeamId>>();
        private readonly Dictionary<TRiderId, List<TTeamId>> riderToTeamMap;

        public RelayRaceRoundResultStrategy(List<(TRiderId,TTeamId)> riderToTeamMap)
        {
            this.riderToTeamMap = riderToTeamMap
                .GroupBy(x => x.Item1)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Item2).ToList());
        }

        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(Checkpoint<TRiderId> value)
        {
            // Нет валидации. Нужно добавить проверки
            // на двойной проезд и тп
            // также нужно отсекать первый проезд после смены гонщика
            foreach (var team in riderToTeamMap.Get(value.RiderId, emptyTeamIds))
            {
                teamCheckpoints.OnNext(new Checkpoint<TTeamId>(team, value.Timestamp));
            }
        }

        public IDisposable Subscribe(IObserver<Checkpoint<TTeamId>> observer)
        {
            return teamCheckpoints.Subscribe(observer);
        }
    }
}