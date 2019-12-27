using System;
using System.Collections.Generic;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using Xunit;

namespace maxbl4.Race.Tests.CheckpointService
{
    public class DeserializationTests
    {
        string str = @"[{'timestamp':'2019-11-04T18:37:34.773Z','riderId':'stored1','id':1},{'timestamp':'2019-11-04T18:39:14.773Z','riderId':'stored2','id':2}]"
            .Replace('\'', '"');
        
        [Fact]
        public void System_Json_should_fail_to_deserialize_checkpoints_array()
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<List<Checkpoint>>(str.Replace('\'', '"'));
            result.Count.Should().Be(2);
            result[0].RiderId.Should().BeNull();
            // Here are the proper result, you could expect. But actually System.Text.Json.JsonSerializer returns nulls here
//            result[0].RiderId.Should().Be("stored1");
//            result[0].Sequence.Should().Be(1);
//            result[0].Timestamp.Should().Be(new DateTime(2019, 11, 04, 18, 37, 34, 773, DateTimeKind.Utc));
        }
        
        [Fact]
        public void Newtonsoft_Json_should_deserialize_checkpoints_array()
        {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Checkpoint>>(str);
            result.Count.Should().Be(2);
            result[0].RiderId.Should().Be("stored1");
            result[0].Id.Should().Be(1);
            result[0].Timestamp.Should().Be(new DateTime(2019, 11, 04, 18, 37, 34, 773, DateTimeKind.Utc));
        }
    }
}