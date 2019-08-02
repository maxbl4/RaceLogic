using System.Collections.Generic;

namespace maxbl4.RaceLogic.RoundTiming
{
    public interface IFinishCriteria
    {
        bool HasFinished(RoundPosition current, IEnumerable<RoundPosition> sequence, bool finishForced);
    }
}