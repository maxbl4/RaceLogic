using RaceLogic.Pipeline;
using RaceLogic.ReferenceModel;
using Xunit;

namespace RaceLogic.Tests
{
    public class PipelineTests
    {
        [Fact]
        public void Should_be_able_to_setup_pipeline()
        {
            var pp = new PipelineConfig<string>();
            //pp.AddInput(manualInput);
        }
    }
}