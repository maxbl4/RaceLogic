using System;
using System.Reactive.PlatformServices;

namespace maxbl4.Race.Tests
{
    public class FakeSystemClock : ISystemClock
    {
        private DateTime now = new(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private bool useRealClock;

        public DateTime Now
        {
            get
            {
                if (useRealClock)
                    return DateTime.UtcNow;
                return now;
            }
            set
            {
                useRealClock = false;
                now = value;
            }
        }

        public DateTimeOffset UtcNow => Now;

        public void UseRealClock()
        {
            useRealClock = true;
        }

        public DateTime Advance(TimeSpan? by = null)
        {
            if (by == null) by = TimeSpan.FromSeconds(1);
            Now = Now.Add(by.Value);
            return Now;
        }
    }
}