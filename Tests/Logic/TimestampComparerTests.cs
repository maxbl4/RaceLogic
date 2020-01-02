using System;
using System.Collections.Generic;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.RoundTiming.Serialization;
using Xunit;

namespace maxbl4.Race.Tests.Logic
{
    public class TimestampComparerTests
    {
        [Fact]
        public void Compare_should_work()
        {
            var ts1 = new DateTime(1, 1, 1, 1, 1, 1);
            var ts2 = new DateTime(1, 1, 1, 1, 2, 1);
            ts1.CompareTo(ts2).Should().BeLessThan(0);
            var cp1 = new Checkpoint("1", ts1);
            var cp2 = new Checkpoint("1", ts2);
            Checkpoint.TimestampComparer.Compare(cp1, cp2).Should().BeLessThan(0);
            Checkpoint.TimestampComparer.Compare(cp2, cp1).Should().BeGreaterThan(0);
        }

        [Fact]
        public void Sort_should_work()
        {
            var ts1 = new DateTime(1, 1, 1, 1, 1, 1);
            var ts2 = new DateTime(1, 1, 1, 1, 2, 1);
            var ts3 = new DateTime(1, 1, 1, 1, 3, 1);
            var cp1 = new Checkpoint ("1", ts1);
            var cp2 = new Checkpoint ("1", ts2);
            var cp3 = new Checkpoint ("1", ts3);
            var lst = new List<Checkpoint> { cp2, cp1, cp3};
            lst.Sort(Checkpoint.TimestampComparer);
            lst[0].Should().BeSameAs(cp1);
            lst[1].Should().BeSameAs(cp2);
            lst[2].Should().BeSameAs(cp3);
            
            lst = new List<Checkpoint> { cp3, cp2, cp1};
            lst.Sort(Checkpoint.TimestampComparer);
            lst[0].Should().BeSameAs(cp1);
            lst[1].Should().BeSameAs(cp2);
            lst[2].Should().BeSameAs(cp3);
        }

        [Fact]
        public void Flexible_timespan_parse_should_work()
        {
            TimeSpanExt.Parse(null).Should().Be(TimeSpan.Zero);
            TimeSpanExt.Parse("").Should().Be(TimeSpan.Zero);
            TimeSpanExt.Parse("10").Should().Be(TimeSpan.FromSeconds(10));
            TimeSpanExt.Parse("59").Should().Be(TimeSpan.FromSeconds(59));
            Assert.Throws<FormatException>(() => TimeSpanExt.Parse("60"));
            TimeSpanExt.Parse("1:10").Should().Be(new TimeSpan(0, 0, 1,10));
            TimeSpanExt.Parse("15:10").Should().Be(new TimeSpan(0, 0, 15,10));
            TimeSpanExt.Parse("12:15:10").Should().Be(new TimeSpan(0, 12, 15,10));
        }

        [Fact]
        public void TimeSpan_to_short_string_should_work()
        {
            TimeSpan.FromSeconds(9).ToShortString().Should().Be("9");
            TimeSpan.FromSeconds(59).ToShortString().Should().Be("59");
            TimeSpan.FromSeconds(60).ToShortString().Should().Be("1:0");
            TimeSpan.FromMinutes(60).ToShortString().Should().Be("1:0:0");
        }
    }
}