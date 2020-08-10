using System;
using System.Collections.Generic;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.RoundTiming
{
    public interface ITrackOfCheckpoints
    {
        DateTime RoundStartTime { get; }
        List<RoundPosition> Rating { get; }
        IFinishCriteria FinishCriteria { get; }
        List<List<Checkpoint>> Track { get; }
        List<Checkpoint> Checkpoints { get; }
        void Append(Checkpoint cp);
        void ForceFinish();
    }
}