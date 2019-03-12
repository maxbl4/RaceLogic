using System;
using System.Collections.Generic;
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
        public void Start_two_times()
        {
            Everyone_started_and_finished();
            Everyone_started_and_finished();
        }

        [Fact]
        public void Dnf_should_be_last_in_the_rating()
        {
            session1.Start();
            session1.Log(11, 1);
            session1.Log(11, 2);
            session1.Log(11, 3);
            session1.Log(12, 4);
            session1.Log(12, 5);
            session1.Log(13, 6);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(3, 2, 1);
            session1.LogFinish(11, 1);
            session1.LogFinish(13, 2);
            ValidateRiderPositions(11, 13, 12);
            ValidateRiderLaps(4, 2, 2);
        }

        [Fact]
        public void Dnf_should_be_last_in_the_rating_2()
        {
            session1.Start();
            session1.Log(11, 1);
            session1.LogFinish(12, 2);
            ValidateRiderPositions(12, 11, 13);
            ValidateRiderLaps(1, 1, 0);
        }
        
        [Fact]
        public void The_only_lap_after_finish_should_count()
        {
            session1.Start();
            session1.Log(11, 1);
            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 1, 0);
            session1.Rating[0].Finished.ShouldBeTrue();
            session1.Rating[1].Finished.ShouldBeTrue();
            session1.Rating[2].Finished.ShouldBeFalse();
        }
        
        [Fact]
        public void The_only_lap_after_finish_should_count_for_unknown_rider()
        {
            session1.Start();
            session1.Log(11, 1);
            session1.LogFinish(11, 2);
            session1.LogFinish(112, 3);
            ValidateRiderPositions(11, 112, 12, 13);
            ValidateRiderLaps(2, 1, 0, 0);
            session1.Rating[0].Finished.ShouldBeTrue();
            session1.Rating[1].Finished.ShouldBeTrue();
            session1.Rating[2].Finished.ShouldBeFalse();
        }

        [Fact]
        public void Start_without_gates_should_have_rating_with_riders_ordered_by_number()
        {
            session1.Start();
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(0, 0, 0);
        }

        [Fact]
        public void In_tight_for_laps_leader_should_found_by_time()
        {
            session1.Start();
            session1.Log(11, 1);
            session1.Log(11, 2);
            session1.Log(12, 4);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 1, 0);
            session1.LogFinish(12, 1);
            ValidateRiderPositions(11, 12, 13);
            session1.Rating[0].Finished.ShouldBeFalse();
            session1.Rating[1].Finished.ShouldBeFalse();
            ValidateRiderLaps(2, 2, 0);
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

        [Fact]
        public void Should_support_calculation_without_timestamps()
        {
            throw new NotSupportedException();
        }

        [Fact]
        public void Everyone_started_and_finished()
        {
            session1.IsRunning.ShouldBe(false);
            session1.Start();
            session1.IsRunning.ShouldBe(true);
            session1.EveryoneFinished.ShouldBe(false);
            session1.Log(11, 1);
            session1.Log(12, 2);
            session1.Log(13, 3);
            session1.Log(11, 2.1);
            session1.Log(12, 4.2);
            session1.Log(11, 3.1);
            session1.EveryoneFinished.ShouldBe(false);
            session1.IsRunning.ShouldBe(true);
            session1.LogFinish(11, 1);
            session1.LogFinish(12, 2);
            session1.LogFinish(13, 3);
            session1.EveryoneFinished.ShouldBe(true);
            session1.Gates.Count.ShouldBe(9);
            session1.IsRunning.ShouldBe(false);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(4, 3, 2);
            ValidateRiderLapTimes(session1.Rating[0], 1, 2.1, 3.1, sessionDuration.TotalMinutes + 1);
            ValidateRiderLapTimes(session1.Rating[2], 3, sessionDuration.TotalMinutes + 3);
        }

        [Fact]
        public void Only_one_lap_after_leader_has_finished_should_be_counted()
        {
            session1.IsRunning.ShouldBe(false);
            session1.Start();
            session1.IsRunning.ShouldBe(true);
            session1.Log(11, 1);
            session1.Log(12, 2);
            session1.Log(13, 3);
            session1.IsRunning.ShouldBe(true);
            session1.LogFinish(11, 1);
            session1.LogFinish(12, 2);
            session1.LogFinish(13, 3);
            session1.Gates.Count.ShouldBe(6);
            session1.IsRunning.ShouldBe(false);
            session1.Log(11, session1.StartTime + sessionDuration + TimeSpan.FromMinutes(5));
            session1.Gates.Count.ShouldBe(7);
            session1.IsRunning.ShouldBe(false);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);

            session1.LogFinish(13, 6);
            session1.LogFinish(13, 7);
            session1.Gates.Count.ShouldBe(9);
            session1.IsRunning.ShouldBe(false);
            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
        }

        [Fact]
        public void Add_log_in_the_middle_should_work()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);

            session1.Log(11, 3);

            session1.Log(12, 7);
            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);
            session1.Log(11, 1);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(4, 3, 2);
            ValidateRiderLapTimes(session1.Rating[0], 1, 2, 3, sessionDuration.TotalMinutes + 2);
        }

        [Fact]
        public void Log_unknown_numbers_should_works()
        {
            session1.Start();
            session1.Log(111, 2);
            session1.Log(112, 4);
            session1.Log(113, 6);

            ValidateRiderPositions(111, 112, 113, 11, 12, 13);
            ValidateRiderLaps(1, 1, 1, 0, 0, 0);
        }

        [Fact]
        public void Update_number_from_bad_to_good()
        {
            session1.Start();
            session1.Log(111, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);
            
            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);
            session1.Calculate();

            ValidateRiderPositions(12, 13, 111, 11);
            ValidateRiderLaps(2, 2, 1, 1);
            ValidateRiderLapTimes(session1.Rating[2], 2);
            ValidateRiderLapTimes(session1.Rating[3], sessionDuration.TotalMinutes + 2);

            session1.Gates[0].RiderId.ShouldBe(111);
            //session1.Gates[0].Rider.ShouldBeNull();

            session1.Gates[0].RiderId = 11;
            session1.UpdateGate(session1.Gates[0], DateTime.Now);
            //session1.Gates[0].Rider.ShouldNotBeNull();

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[0], 2, sessionDuration.TotalMinutes + 2);
        }

        [Fact]
        public void Update_number_from_good_to_bad()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);

            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[0], 2, sessionDuration.TotalMinutes + 2);

            session1.Gates[0].RiderId.ShouldBe(11);
            //session1.Gates[0].Rider.ShouldNotBeNull();

            session1.Gates[0].RiderId = 111;
            session1.UpdateGate(session1.Gates[0], DateTime.Now);
            //session1.Gates[0].Rider.ShouldBeNull();

            ValidateRiderPositions(12, 13, 111, 11);
            ValidateRiderLaps(2, 2, 1, 1);
            ValidateRiderLapTimes(session1.Rating[2], 2);
            ValidateRiderLapTimes(session1.Rating[3], sessionDuration.TotalMinutes + 2);
        }

        [Fact]
        public void Update_timestamp_inside_race()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);

            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[1], 4, sessionDuration.TotalMinutes + 3);

            session1.Gates[1].Timestamp = session1.StartTime + TimeSpan.FromMinutes(3);
            session1.UpdateGate(session1.Gates[1], DateTime.Now);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[1], 3, sessionDuration.TotalMinutes + 3);
        }

        [Fact]
        public void Update_finish_timestamp()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);

            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[1], 4, sessionDuration.TotalMinutes + 3);

            session1.Gates[4].Timestamp = session1.StartTime + sessionDuration + TimeSpan.FromMinutes(1);
            session1.UpdateGate(session1.Gates[4], DateTime.Now);

            ValidateRiderPositions(12, 11, 13);
            ValidateRiderLaps(2, 2, 2);
            ValidateRiderLapTimes(session1.Rating[0], 4, sessionDuration.TotalMinutes + 1);
        }

        [Fact]
        public void Move_timestamp_outside_finish()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);
            session1.Log(11, 8);
            session1.Log(12, 10);
            session1.Log(13, 12);

            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(3, 3, 3);
            ValidateRiderLapTimes(session1.Rating[1], 4, 10, sessionDuration.TotalMinutes + 3);

            session1.Gates[4].Timestamp = session1.StartTime + sessionDuration + TimeSpan.FromMinutes(10);
            session1.UpdateGate(session1.Gates[4], DateTime.Now);

            ValidateRiderPositions(11, 13, 12);
            ValidateRiderLaps(3, 3, 2);
            ValidateRiderLapTimes(session1.Rating[2], 4, sessionDuration.TotalMinutes + 3);
        }

        [Fact]
        public void Move_timestamp_inside_finish()
        {
            session1.Start();
            session1.Log(11, 2);
            session1.Log(12, 4);
            session1.Log(13, 6);
            session1.Log(11, 8);
            session1.Log(13, 12);

            session1.LogFinish(11, 2);
            session1.LogFinish(12, 3);
            session1.LogFinish(13, 4);
            session1.LogFinish(12, 10);
            session1.LogFinish(12, 10);

            ValidateRiderPositions(11, 13, 12);
            ValidateRiderLaps(3, 3, 2);
            ValidateRiderLapTimes(session1.Rating[2], 4, sessionDuration.TotalMinutes + 3);

            session1.Gates[8].Timestamp = session1.StartTime + TimeSpan.FromMinutes(10);
            session1.UpdateGate(session1.Gates[8], DateTime.Now);

            ValidateRiderPositions(11, 12, 13);
            ValidateRiderLaps(3, 3, 3);
            ValidateRiderLapTimes(session1.Rating[1], 4, 10, sessionDuration.TotalMinutes + 3);
        }


        void ValidateRiderPositions(params int[] RiderIds)
        {
            session1.Rating.Count.ShouldBe(RiderIds.Length);
            for (int i = 0; i < RiderIds.Length; i++)
            {
                var n = RiderIds[i];
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

        void ValidateRiderLapTimes(RoundPosition record, params double[] riderLapTimes)
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
        public DateTimeOffset StartTime { get; } = new DateTime(1, 1, 1, 11, 30, 0);
        public List<Checkpoint<int>> Checkpoints { get; } = new List<Checkpoint<int>>();
        public List<Checkpoint<int>> Gates => Checkpoints;
        public TimeSpan SessionDuration { get; }
        public List<RoundPosition> Rating { get; private set; } = new List<RoundPosition>();

        public Session(TimeSpan sessionDuration)
        {
            SessionDuration = sessionDuration;
        }
        public void Log(int RiderId, double offset)
        {
            Checkpoints.Add(new Checkpoint<int>
            {
                RiderId = RiderId, Timestamp = StartTime + TimeSpan.FromMinutes(offset)
            });
            Calculate();
        }
        public void Log(int RiderId, DateTimeOffset timestamp)
        {
            Checkpoints.Add(new Checkpoint<int>
            {
                RiderId = RiderId, Timestamp = timestamp
            });
            Calculate();
        }
        
        public void LogFinish(int RiderId, double offset)
        {
            Checkpoints.Add(new Checkpoint<int>
            {
                RiderId = RiderId, Timestamp = StartTime + SessionDuration + TimeSpan.FromMinutes(offset)
            });
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

        public void UpdateGate(Checkpoint<int> checkpoint, DateTimeOffset timestamp)
        {
            //checkpoint.Timestamp = timestamp;
            //Checkpoints.Sort(Checkpoint<int>.TimestampComparer);
            Calculate();
        }

        public void Calculate()
        {
            Checkpoints.Sort(Checkpoint<int>.TimestampComparer);
            var strategy = new ClassicRoundResultStrategy<int, Checkpoint<int>, RoundPosition, Lap>();
            var result = strategy.Process(Checkpoints, new HashSet<int>{11,12,13}, StartTime, SessionDuration);
            this.EveryoneFinished = result.EveryoneFinished;
            if (EveryoneFinished)
                IsRunning = false;
            this.Rating = result.Rating;
        }
    }
}