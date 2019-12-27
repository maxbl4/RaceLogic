using System.Collections.Generic;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class AdHoc
    {
        [Fact]
        public void Pattern_Matching_should_work_for_inherited_classes()
        {
            Gestring(new Checkpoint("1")).Should().Be("Checkpoint 1");
            Gestring(AggCheckpoint.From(new Checkpoint("1"))).Should().Be("AggCheckpoint 1");
        }

        [Fact]
        public void Dictionary_comparer()
        {
            var d = new Dictionary<Checkpoint, string>(new MyCheckpointComparer()){
                {new Checkpoint("1"), "111"},
                {AggCheckpoint.From(new Checkpoint("2")), "222"},
            };
            d[new Checkpoint("1")].Should().Be("111");
            d[AggCheckpoint.From(new Checkpoint("1"))].Should().Be("111");
        }

        class MyCheckpointComparer : IEqualityComparer<Checkpoint>
        {
            public bool Equals(Checkpoint x, Checkpoint y)
            {
                return Equals(x?.RiderId, y?.RiderId);
            }

            public int GetHashCode(Checkpoint obj)
            {
                return obj.RiderId.GetHashCode();
            }
        }

        string Gestring(object o)
        {
            switch (o)
            {
                case AggCheckpoint cp:
                    return "AggCheckpoint " + cp.RiderId;
                case Checkpoint cp:
                    return "Checkpoint " + cp.RiderId;
                default:
                    return "";
            }
        }
    }
}