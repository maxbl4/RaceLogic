using FluentAssertions;
using maxbl4.Race.DataService.Services;
using Xunit;

namespace maxbl4.Race.Tests.DataService.Services
{
    public class StorageServiceTests
    {
        [Fact]
        public void Should_parse_order()
        {
            StorageService.TryParseOrder(null, out _).Should().BeFalse();
            StorageService.TryParseOrder("", out _).Should().BeFalse();
            StorageService.TryParseOrder("A", out var order).Should().BeTrue();
            order.Should().Be(("A", false));
            StorageService.TryParseOrder("-B", out order).Should().BeTrue();
            order.Should().Be(("B", true));
        }
    }
}