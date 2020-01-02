using System;
using System.Globalization;

namespace maxbl4.Race.Logic.RoundTiming.Serialization
{
    public static class TimeSpanExt
    {
        public static TimeSpan Parse(string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return TimeSpan.Zero;
            return TimeSpan.ParseExact(src, new[]
            {
                @"%h\:%m\:%s",
                @"%m\:%s",
                @"%s"
            }, CultureInfo.InvariantCulture);
        }

        public static string ToShortString(this TimeSpan ts)
        {
            if (ts.TotalSeconds <= 59) return ts.ToString("%s");
            if (ts.TotalMinutes < 60) return ts.ToString(@"%m\:%s");
            return ts.ToString(@"%h\:%m\:%s");
        }
    }
}