using maxbl4.Race.Logic.EventModel.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Logic.EventModel.EventModel
{
    public class TimingSessionTests: IntegrationTestBase
    {
        public TimingSessionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
        
        [Fact]
        public void Start_session()
        {
            //var timingService = new TimingSessionService();
        }
    }
}