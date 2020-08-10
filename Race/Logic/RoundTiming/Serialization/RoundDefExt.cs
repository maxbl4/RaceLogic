using System;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
{
    public static class RoundDefExt
    {
        public static ITrackOfCheckpoints CreateTrack(this RoundDef def, FinishCriteria fc)
        {
            var track = TrackOfCheckpointsFactory.Create(def.RoundStartTime, fc);
            foreach (var checkpoint in def.Checkpoints)
                track.Append(checkpoint);
            return track;
        }
        
        public static ITrackOfCheckpoints CreateTrack(this RoundDef def, Func<DateTime?, IFinishCriteria, ITrackOfCheckpoints>
            factory, FinishCriteria fc)
        {
            var track = factory(def.RoundStartTime, fc);
            foreach (var checkpoint in def.Checkpoints)
                track.Append(checkpoint);
            return track;
        }
    }
}