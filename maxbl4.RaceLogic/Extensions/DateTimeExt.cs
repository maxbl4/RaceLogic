using System;

namespace maxbl4.RaceLogic.Extensions
{
    public static class DateTimeExt
    {
        public static DateTime TakeSmaller(this DateTime current, DateTime other)
        {
            if (current == default(DateTime))
                return other;
            if (other == default(DateTime))
                return current;
            return current < other ? current : other;
        }
        
        public static DateTime TakeLarger(this DateTime current, DateTime other)
        {
            if (current == default(DateTime))
                return other;
            if (other == default(DateTime))
                return current;
            return current > other ? current : other;
        }
    }
}