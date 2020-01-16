using System;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Race.Logic.EventModel;
using Xunit;
using maxbl4.Race.Logic.EventModel.Traits;

namespace maxbl4.Race.Tests.Logic.EventModel.Traits
{
    public class TraitsExtTests
    {
        [Fact]
        public async Task Should_apply_traits()
        {
            var series = new SeriesDef();
            series.Id.IsEmpty.Should().BeTrue();
            series.Created.Should().Be(default);
            series.Updated.Should().Be(default);
            series.ApplyTraits();
            series.Id.Value.Should().NotBeEmpty();
            series.Created.Should().BeCloseTo(DateTime.UtcNow);
            series.Updated.Should().BeCloseTo(DateTime.UtcNow);
            var id = series.Id;
            var created = series.Created;
            var updated = series.Updated;
            await Task.Delay(100);
            var s = series.ApplyTraits();
            s.Should().BeSameAs(series);
            s.Id.Should().Be(id);
            s.Created.Should().Be(created);
            s.Updated.Should().BeAfter(updated);
        }
    }
}