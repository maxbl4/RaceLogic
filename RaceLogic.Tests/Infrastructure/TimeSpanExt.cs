using System;
using System.Globalization;

namespace RaceLogic.Tests.Infrastructure
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
                @"%s",
            }, CultureInfo.InvariantCulture);
        }
    }
}