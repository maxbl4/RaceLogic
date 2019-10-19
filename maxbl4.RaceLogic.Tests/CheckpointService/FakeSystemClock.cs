using System;
using System.Reactive.PlatformServices;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class FakeSystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => Now;
        public DateTime Now { get; set; } = new DateTime(2019, 1,1, 0, 0, 0, DateTimeKind.Utc);

        public DateTime Advance(TimeSpan? by = null)
        {
            if (by == null) by = TimeSpan.FromSeconds(1);
            Now = Now.Add(by.Value);
            return Now;
        }
    }
}