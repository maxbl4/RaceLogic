using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using maxbl4.RaceLogic.Checkpoints;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        [Fact]
        public void Dictionary_comparer()
        {
            var d = new Dictionary<Checkpoint<string>, string>(new MyCheckpointComparer()){
                {new Checkpoint<string>("1"), "111"},
                {AggCheckpoint<string>.From(new Checkpoint<string>("2")), "222"},
            };
            d[new Checkpoint<string>("1")].ShouldBe("111");
            d[AggCheckpoint<string>.From(new Checkpoint<string>("1"))].ShouldBe("111");
        }

        class MyCheckpointComparer : IEqualityComparer<Checkpoint<string>>
        {
            public bool Equals(Checkpoint<string> x, Checkpoint<string> y)
            {
                return Equals(x?.RiderId, y?.RiderId);
            }

            public int GetHashCode(Checkpoint<string> obj)
            {
                return obj.RiderId.GetHashCode();
            }
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