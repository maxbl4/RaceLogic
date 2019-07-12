using System;
using System.Collections.Generic;
using System.IO;
using maxbl4.RaceLogic.LogManagement;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.Infrastructure
{
    public class JsonTests
    {
        [Fact]
        public void Should_serialize_json_as_single_line()
        {
            var s = new JsonSerializer();
            s.DefaultValueHandling = DefaultValueHandling.Ignore;
            s.SerializationBinder = new NameMapSerializationBinder(new Dictionary<string, Type>(){
                {"entity", typeof(Entity)}});
            s.TypeNameHandling = TypeNameHandling.All;
            var sw = new StringWriter();
            s.Serialize(sw, new Entity{ Data = "some" });
            sw.ToString().ShouldBe("{'$type':'entity','Data':'some','number':null}".Replace('\'', '"'));
            sw = new StringWriter();
            var ts = new DateTime(2019, 08, 10, 1, 2, 3, DateTimeKind.Utc)
                .AddMilliseconds(456);
            s.Serialize(sw, new Entity{ Data = "some\r\nline", Timestamp = 
                ts});
            sw.ToString().ShouldBe(@"{'$type':'entity','Data':'some\r\nline','number':null,'Timestamp':'2019-08-10T01:02:03.456Z'}".Replace('\'', '"'));
            var t = s.Deserialize(new JsonTextReader(new StringReader(sw.ToString())))
                as Entity;
            t.ShouldNotBeNull();
            t.Data.ShouldBe("some\r\nline");
            t.Timestamp.Kind.ShouldBe(DateTimeKind.Utc);
            t.Timestamp.ShouldBe(ts);
        }

        class Entity
        {
            public string Data { get; set; }
            public int DefaultInt { get; set; }
            [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Include)]
            public int? ExplicitInt { get; set; }

            public DateTime Timestamp { get; set; }
        }
    }
}