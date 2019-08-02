using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Extensions;

namespace maxbl4.RaceLogic
{
    public class RelayRaceRoundResultStrategy : IObserver<Checkpoint>,
        IObservable<Checkpoint>
    {
        private readonly List<string> emptyTeamIds = new List<string>();
        private readonly Subject<Checkpoint> teamCheckpoints = new Subject<Checkpoint>();
        private readonly Dictionary<string, List<string>> riderToTeamMap;

        public RelayRaceRoundResultStrategy(List<(string,string)> riderToTeamMap)
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

        public void OnNext(Checkpoint value)
        {
            // Нет валидации. Нужно добавить проверки
            // на двойной проезд и тп
            // также нужно отсекать первый проезд после смены гонщика
            foreach (var team in riderToTeamMap.Get(value.RiderId, emptyTeamIds))
            {
                teamCheckpoints.OnNext(new Checkpoint(team, value.Timestamp));
            }
        }

        public IDisposable Subscribe(IObserver<Checkpoint> observer)
        {
            return teamCheckpoints.Subscribe(observer);
        }
    }
}