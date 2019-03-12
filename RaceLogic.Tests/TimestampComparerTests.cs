using System;
using System.Collections.Generic;
using System.Globalization;
using RaceLogic.ReferenceModel;
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
            var ts1 = new DateTimeOffset(1, 1, 1, 1, 1, 1, TimeSpan.Zero);
            var ts2 = new DateTimeOffset(1, 1, 1, 1, 2, 1, TimeSpan.Zero);
            ts1.CompareTo(ts2).ShouldBeLessThan(0);
            var cp1 = new Checkpoint<int> {Timestamp = ts1};
            var cp2 = new Checkpoint<int> {Timestamp = ts2};
            Checkpoint<int>.TimestampComparer.Compare(cp1, cp2).ShouldBeLessThan(0);
            Checkpoint<int>.TimestampComparer.Compare(cp2, cp1).ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Sort_should_work()
        {
            var ts1 = new DateTimeOffset(1, 1, 1, 1, 1, 1, TimeSpan.Zero);
            var ts2 = new DateTimeOffset(1, 1, 1, 1, 2, 1, TimeSpan.Zero);
            var ts3 = new DateTimeOffset(1, 1, 1, 1, 3, 1, TimeSpan.Zero);
            var cp1 = new Checkpoint<int> {Timestamp = ts1};
            var cp2 = new Checkpoint<int> {Timestamp = ts2};
            var cp3 = new Checkpoint<int> {Timestamp = ts3};
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
            TimeSpanExt.Parse("1:10").ShouldBe(new TimeSpan(0, 0, 1,10));
            TimeSpanExt.Parse("15:10").ShouldBe(new TimeSpan(0, 0, 15,10));
            TimeSpanExt.Parse("12:15:10").ShouldBe(new TimeSpan(0, 12, 15,10));
        }
    }
}