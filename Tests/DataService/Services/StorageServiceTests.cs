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
            StorageService.ParseOrder(null).ShouldBeEmpty();
            StorageService.ParseOrder("").ShouldBeEmpty();
            StorageService.ParseOrder("A").ShouldContain(x => x.field == "A" && x.desc == false);
            StorageService.ParseOrder("B desc").ShouldContain(x => x.field == "B" && x.desc == true);
            var order = StorageService.ParseOrder("A, B desc, C\tsome, D+asc,E+desc").ToList();
            order[0].ShouldBe(("A", false));
            order[1].ShouldBe(("B", true));
            order[2].ShouldBe(("C", false));
            order[3].ShouldBe(("D", false));
            order[4].ShouldBe(("E", true));
        }
    }
}