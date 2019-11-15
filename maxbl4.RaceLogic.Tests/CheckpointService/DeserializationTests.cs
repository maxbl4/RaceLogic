using System;
using System.Collections.Generic;
using System.IO;
using maxbl4.RaceLogic.Checkpoints;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService
{
    public class DeserializationTests
    {
        string str = @"[{'timestamp':'2019-11-04T18:37:34.773Z','riderId':'stored1','sequence':1},{'timestamp':'2019-11-04T18:39:14.773Z','riderId':'stored2','sequence':2}]"
            .Replace('\'', '"');
        
        [Fact]
        public void System_Json_should_fail_to_deserialize_checkpoints_array()
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<List<Checkpoint>>(str.Replace('\'', '"'));
            result.Count.ShouldBe(2);
            result[0].RiderId.ShouldBeNull();
            // Here are the proper result, you could expect. But actually System.Text.Json.JsonSerializer returns nulls here
//            result[0].RiderId.ShouldBe("stored1");
//            result[0].Sequence.ShouldBe(1);
//            result[0].Timestamp.ShouldBe(new DateTime(2019, 11, 04, 18, 37, 34, 773, DateTimeKind.Utc));
        }
        
        [Fact]
        public void Newtonsoft_Json_should_deserialize_checkpoints_array()
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Checkpoint>>(str);
            result.Count.ShouldBe(2);
            result[0].RiderId.ShouldBe("stored1");
            result[0].Sequence.ShouldBe(1);
            result[0].Timestamp.ShouldBe(new DateTime(2019, 11, 04, 18, 37, 34, 773, DateTimeKind.Utc));
        }
    }
}