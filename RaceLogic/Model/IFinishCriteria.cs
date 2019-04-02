using System;
using System.Collections.Generic;

namespace RaceLogic.Model
{
    public interface IFinishCriteria
    {
        bool HasFinished<TRiderId>(RoundPosition<TRiderId> current, IEnumerable<RoundPosition<TRiderId>> sequence, bool finishForced)
            where TRiderId: IEquatable<TRiderId>;
    }
}