using System;
using RaceLogic.Model;

namespace RaceLogic.Extensions
{
    public static class RoundPositionExt
    {
        public static int IndexOfStartAndFinish<TRiderId>(this RoundPosition<TRiderId> position)
            where TRiderId: IEquatable<TRiderId>
        {
            if (position.Finished) return 2;
            if (position.Started) return 1;
            return 0;
        }
    }
}