using maxbl4.RaceLogic.RoundTiming;
using maxbl4.RaceLogic.Tests.Infrastructure;
using Xunit;

namespace maxbl4.RaceLogic.Tests.Model
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
        public void Force_finish_when_lap_leader_is_down()
        {
            var def = RoundDef.Parse(@"Track 30
11[1] 11[2] 11[3] 12[4]
12[34]
Rating
F12 2 [4 34]
11 3 [1 2 3]");
            var fc = FinishCriteria.FromDuration(def.Duration);
            var track = def.CreateTrack(fc);
            track.ForceFinish();
            def.VerifyTrack(track);
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
        
        [Fact]
        public void Dnf_should_be_last_in_the_rating_2()
        {
            var def = RoundDef.Parse(@"Track 30
11[1] 12[2] 11[3] 12[4] 11[5] 12[6]
13[7]
11[31] 13[32] 
Rating
F11 4 [1 3 5 31]
F13 2 [7 32]
 12 3 [2 4 6]");
            var track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            def.VerifyTrack(track);
        }
        
        [Fact]
        public void Sequence_of_checkpoints_should_prevail_over_timestamps()
        {
            var def = RoundDef.Parse(@"Track 30
11[10] 12[8]
11[40] 12[32]
Rating
F11 2 [10 40]
F12 2 [8 32]");
            var fc = FinishCriteria.FromDuration(def.Duration);
            var track = def.CreateTrack(fc);
            def.VerifyTrack(track);
        }
        
        [Fact]
        public void Force_finish_should_not_affect_rating_with_normal_finish()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10] 13[15]
11[30] 12[32]
Rating
F11 2 [5  30]
F12 2 [10 32]
 13 1 [15]");
            var track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            track.ForceFinish();
            def.VerifyTrack(track);
        }
        
        [Fact]
        public void Checkpoints_after_finish_should_be_ignored()
        {
            var def = RoundDef.Parse(@"Track 30
11[5]  12[10]
11[30] 12[32]
11[35] 12[37]
Rating
F11 2 [5  30]
F12 2 [10 32]");
            var track = def.CreateTrack(FinishCriteria.FromDuration(def.Duration));
            track.ForceFinish();
            def.VerifyTrack(track);
        }
    }
}