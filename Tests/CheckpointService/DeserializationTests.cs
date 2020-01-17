using System;
using System.Collections.Generic;
using FluentAssertions;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace maxbl4.Race.Tests.CheckpointService
{
    public class DeserializationTests
    {
        string str = @"[{'timestamp':'2019-11-04T18:37:34.773Z','riderId':'stored1','id':'08d79b60-0191-f7da-9d44-2371e4be9b71'},{'timestamp':'2019-11-04T18:39:14.773Z','riderId':'stored2', 'id':'sdfsdf'}]"
            .Replace('\'', '"');
        
        [Fact]
        public void System_Json_should_fail_to_deserialize_checkpoints_array()
        {
            var result = JsonSerializer.Deserialize<List<Checkpoint>>(str);
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
            var result = JsonConvert.DeserializeObject<List<Checkpoint>>(str);
            result.Count.Should().Be(2);
            result[0].RiderId.Should().Be("stored1");
            result[0].Id.Should().Be(new Id<Checkpoint>(new Guid("08d79b60-0191-f7da-9d44-2371e4be9b71")));
            result[0].Timestamp.Should().Be(new DateTime(2019, 11, 04, 18, 37, 34, 773, DateTimeKind.Utc));
            result[1].Id.Value.Should().BeEmpty();
        }
    }
}