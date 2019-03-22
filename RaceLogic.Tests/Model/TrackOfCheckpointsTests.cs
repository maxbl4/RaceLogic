using System;
using System.Linq;
using RaceLogic.Model;
using RaceLogic.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests.Model
{
    public class TrackOfCheckpointsTests
    {
        [Fact]
        public void Simple_start_and_finish()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[30] 12[32] 13[33]
Rating
F11 2 [5  30]
F12 2 [10 32]
F13 2 [15 33]");
            var track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            def.VerifyTrack(track);
        }
        
        [Fact]
        public void In_lack_of_timestamps_should_set_finish_to_leader()
        {
//            Реализовать подсчёт без меток времени в виде отдельного класса!
//            Это частный случай, который сильно отличается от всех вариантов с метками времени.
//            В остальных местах требовать ненулевые метки
            var def = RoundDef.Parse(@"Track
11 12 13
12 11
Rating
F12 2 [1 32]
F11 2 [2 31]
 13 1 [3]");
            var fc = FinishCriteria.FromForcedFinish();
            var track = def.CreateTrack(fc);
            track.ForceFinish();
            def.VerifyTrack(track, false);
        }
        
        [Fact]
        public void Dnf_should_be_last_in_the_rating()
        {
            var def = RoundDef.Parse(@"Track 2018-01-15 30
11[1]  11[2] 11[3] 12[4] 12[5] 13[6]
Rating
11 3 [1 2 3]
12 2 [4 5]
13 1 [6]");
            var track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            def.VerifyTrack(track);
            def = RoundDef.Parse(@"Track 2018-01-15 30
11[1] 11[2] 11[3] 12[4] 12[5] 13[6]
11[31] 13[32]
Rating
F11 4 [1 2 3 31]
F13 2 [6 32]
 12 2 [4 5]");
            track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            def.VerifyTrack(track);
        }
        
    }
}