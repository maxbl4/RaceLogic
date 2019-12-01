using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.Extensions
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