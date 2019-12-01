using maxbl4.Race.Tests.Ext;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class TestOutputHelperTests
    {
        private readonly ITestOutputHelper outputHelper;

        public TestOutputHelperTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            outputHelper.GetTestName().ShouldBe("maxbl4.Race.Tests.Infrastructure.TestOutputHelperTests.Should_get_executing_test_name");
        }

        [Fact]
        public void Should_get_executing_test_name()
        {
            outputHelper.GetTestName().ShouldBe("maxbl4.Race.Tests.Infrastructure.TestOutputHelperTests.Should_get_executing_test_name");
        }
    }
}