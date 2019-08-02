using maxbl4.RaceLogic.RoundTiming;

namespace maxbl4.RaceLogic.Extensions
{
    public static class RoundPositionExt
    {
        public static int IndexOfStartAndFinish(this RoundPosition position)
        {
            if (position.Finished) return 2;
            if (position.Started) return 1;
            return 0;
        }
    }
}