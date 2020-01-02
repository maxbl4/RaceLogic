using System;
using System.Reactive.PlatformServices;

namespace maxbl4.Race.Tests
{
    public class FakeSystemClock : ISystemClock
    {
        private bool useRealClock;
        private DateTime now = new DateTime(2019, 1,1, 0, 0, 0, DateTimeKind.Utc);
        public DateTimeOffset UtcNow => Now;

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