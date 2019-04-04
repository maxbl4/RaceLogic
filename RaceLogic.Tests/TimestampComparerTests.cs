using System;
using System.Collections.Generic;
using RaceLogic.Checkpoints;
using RaceLogic.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace RaceLogic.Tests
{
    public class TimestampComparerTests
    {
        [Fact]
        public void Compare_should_work()
        {
            var ts1 = new DateTime(1, 1, 1, 1, 1, 1);
            var ts2 = new DateTime(1, 1, 1, 1, 2, 1);
            ts1.CompareTo(ts2).ShouldBeLessThan(0);
            var cp1 = new Checkpoint<int>(1, ts1);
            var cp2 = new Checkpoint<int>(1, ts2);
            Checkpoint<int>.TimestampComparer.Compare(cp1, cp2).ShouldBeLessThan(0);
            Checkpoint<int>.TimestampComparer.Compare(cp2, cp1).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Sort_should_work()
        {
            var ts1 = new DateTime(1, 1, 1, 1, 1, 1);
            var ts2 = new DateTime(1, 1, 1, 1, 2, 1);
            var ts3 = new DateTime(1, 1, 1, 1, 3, 1);
            var cp1 = new Checkpoint<int> (1, ts1);
            var cp2 = new Checkpoint<int> (1, ts2);
            var cp3 = new Checkpoint<int> (1, ts3);
            var lst = new List<Checkpoint<int>> { cp2, cp1, cp3};
            lst.Sort(Checkpoint<int>.TimestampComparer);
            lst[0].ShouldBeSameAs(cp1);
            lst[1].ShouldBeSameAs(cp2);
            lst[2].ShouldBeSameAs(cp3);
            
            lst = new List<Checkpoint<int>> { cp3, cp2, cp1};
            lst.Sort(Checkpoint<int>.TimestampComparer);
            lst[0].ShouldBeSameAs(cp1);
            lst[1].ShouldBeSameAs(cp2);
            lst[2].ShouldBeSameAs(cp3);
        }

        [Fact]
        public void Flexible_timespan_parse_should_work()
        {
            TimeSpanExt.Parse(null).ShouldBe(TimeSpan.Zero);
            TimeSpanExt.Parse("").ShouldBe(TimeSpan.Zero);
            TimeSpanExt.Parse("10").ShouldBe(TimeSpan.FromSeconds(10));
            TimeSpanExt.Parse("59").ShouldBe(TimeSpan.FromSeconds(59));
            Assert.Throws<FormatException>(() => TimeSpanExt.Parse("60"));
            TimeSpanExt.Parse("1:10").ShouldBe(new TimeSpan(0, 0, 1,10));
            TimeSpanExt.Parse("15:10").ShouldBe(new TimeSpan(0, 0, 15,10));
            TimeSpanExt.Parse("12:15:10").ShouldBe(new TimeSpan(0, 12, 15,10));
        }

        [Fact]
        public void TimeSpan_to_short_string_should_work()
        {
            TimeSpan.FromSeconds(9).ToShortString().ShouldBe("9");
            TimeSpan.FromSeconds(59).ToShortString().ShouldBe("59");
            TimeSpan.FromSeconds(60).ToShortString().ShouldBe("1:0");
            TimeSpan.FromMinutes(60).ToShortString().ShouldBe("1:0:0");
        }
    }
}