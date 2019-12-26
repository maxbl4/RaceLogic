using System.Linq;
using maxbl4.Race.DataService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.DataService.Services
{
    public class StorageServiceTests
    {
        [Fact]
        public void Should_parse_order()
        {
            StorageService.TryParseOrder(null, out _).ShouldBeFalse();
            StorageService.TryParseOrder("", out _).ShouldBeFalse();
            StorageService.TryParseOrder("A", out var order).ShouldBeTrue();
            order.ShouldBe(("A", false));
            StorageService.TryParseOrder("-B", out order).ShouldBeTrue();
            order.ShouldBe(("B", true));
        }
    }
}