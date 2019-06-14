using maxbl4.RaceLogic.Checkpoints;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.Infrastructure
{
    public class AdHoc
    {
        [Fact]
        public void Pattern_Matching_should_work_for_inherited_classes()
        {
            GetRiderId(new Checkpoint<int>(1)).ShouldBe("Checkpoint 1");
            GetRiderId(AggCheckpoint<int>.From(new Checkpoint<int>(1))).ShouldBe("AggCheckpoint 1");
        }

        string GetRiderId(object o)
        {
            switch (o)
            {
                case AggCheckpoint<int> cp:
                    return "AggCheckpoint " + cp.RiderId;
                case Checkpoint<int> cp:
                    return "Checkpoint " + cp.RiderId;
                default:
                    return "";
            }
        }
    }
}