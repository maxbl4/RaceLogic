using System;
using System.Collections.Generic;
using RaceLogic.Checkpoints;
using RaceLogic.Model;
using RaceLogic.ReferenceModel;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class ClassicRoundResultStrategyTests
    {
        private Session session1;
        private TimeSpan sessionDuration = TimeSpan.FromMinutes(45);

        public ClassicRoundResultStrategyTests()
        {
            session1 = new Session(sessionDuration);
        }
        
        [Fact]
        public void Start_without_gates_should_have_rating_with_riders_ordered_by_number()
        {
            session1.Start();
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(0, 0, 0);
        }

        [Fact]
        public void Dns_should_be_last_in_the_rating()
        {
            session1.Start();
            session1.LogFinish(11, 1);
            session1.LogFinish(12, 2);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(1, 1, 0);
            ValidateRiderLapTimes(session1.Rating[2]);
        }

        void ValidateRiderPositions(params int[] riderIds)
        {
            session1.Rating.Count.ShouldBe(riderIds.Length);
            for (int i = 0; i < riderIds.Length; i++)
            {
                var n = riderIds[i];
                session1.Rating[i].RiderId.ShouldBe(n, $"Rider {n} should be on position {i + 1}. Actual {session1.Rating[i].RiderId}");
                session1.Rating[i].Position.ShouldBe(i + 1);
            }
        }

        void ValidateRiderLaps(params int[] riderLaps)
        {
            session1.Rating.Count.ShouldBe(riderLaps.Length);
            for (int i = 0; i < riderLaps.Length; i++)
            {
                var l = riderLaps[i];
                session1.Rating[i].Laps.Count.ShouldBe(l, $"Rider {session1.Rating[i].RiderId} should have {l} laps. Actual {session1.Rating[i].Laps.Count}");
            }
        }

        void ValidateRiderLapTimes(RoundPosition<int> record, params double[] riderLapTimes)
        {
            record.Laps.Count.ShouldBe(riderLapTimes.Length);
            for (int i = 0; i < riderLapTimes.Length; i++)
            {
                var t = riderLapTimes[i];
                record.Laps[i].End.ShouldBe(session1.StartTime + TimeSpan.FromMinutes(t));
            }
        }
    }
    
    public class Session
    {
        public bool IsRunning { get; private set; }
        public bool EveryoneFinished { get; private set; }
        public DateTime StartTime { get; } = new DateTime(1, 1, 1, 11, 30, 0);
        public List<Checkpoint<int>> Checkpoints { get; } = new List<Checkpoint<int>>();
        public List<Checkpoint<int>> Gates => Checkpoints;
        public TimeSpan SessionDuration { get; }
        public List<RoundPosition<int>> Rating { get; private set; } = new List<RoundPosition<int>>();

        public Session(TimeSpan sessionDuration)
        {
            SessionDuration = sessionDuration;
        }
        public void Log(int riderId, double offset)
        {
            Checkpoints.Add(new Checkpoint<int>(riderId, StartTime + TimeSpan.FromMinutes(offset)));
            Calculate();
        }
        public void Log(int riderId, DateTime timestamp)
        {
            Checkpoints.Add(new Checkpoint<int>(riderId, timestamp));
            Calculate();
        }
        
        public void LogFinish(int RiderId, double offset)
        {
            Checkpoints.Add(new Checkpoint<int>(RiderId, StartTime + SessionDuration + TimeSpan.FromMinutes(offset)));
            Calculate();
        }

        public void Start()
        {
            Checkpoints.Clear();
            Rating.Clear();
            IsRunning = true;
            EveryoneFinished = false;
            Calculate();
        }
        
        public void Stop()
        {
            Calculate(true);
        }

        public void UpdateGate(Checkpoint<int> checkpoint, DateTime timestamp)
        {
            Calculate();
        }

        public void Calculate(bool forcedFinish = false)
        {
            Checkpoints.Sort(Checkpoint<int>.TimestampComparer);
            var strategy = new ClassicRoundResultStrategy<int>();
            var result = strategy.Process(Checkpoints, new HashSet<int>{11,12,13}, StartTime, SessionDuration, forcedFinish);
            this.EveryoneFinished = result.EveryoneFinished;
            if (EveryoneFinished)
                IsRunning = false;
            this.Rating = result.Rating;
        }
    }
}