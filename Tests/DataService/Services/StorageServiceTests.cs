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
            DataServiceRepository.TryParseOrder(null, out _).Should().BeFalse();
            DataServiceRepository.TryParseOrder("", out _).Should().BeFalse();
            DataServiceRepository.TryParseOrder("A", out var order).Should().BeTrue();
            order.Should().Be(("A", false));
            DataServiceRepository.TryParseOrder("-B", out order).Should().BeTrue();
            order.Should().Be(("B", true));
        }
    }
}