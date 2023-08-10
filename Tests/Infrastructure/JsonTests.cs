using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using maxbl4.Race.Logic.Extensions;
using maxbl4.Race.Logic.LogManagement;
using Newtonsoft.Json;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class JsonTests
    {
        [Fact]
        public void Should_serialize_json_as_single_line()
        {
            var s = new JsonSerializer();
            s.DefaultValueHandling = DefaultValueHandling.Ignore;
            s.SerializationBinder = new NameMapSerializationBinder(new Dictionary<string, Type>
            {
                {"entity", typeof(Entity)}
            });
            s.TypeNameHandling = TypeNameHandling.All;
            var sw = new StringWriter();
            s.Serialize(sw, new Entity {Data = "some"});
            sw.ToString().Should().Be("{'$type':'entity','Data':'some','number':null}".Replace('\'', '"'));
            sw = new StringWriter();
            var ts = new DateTime(2019, 08, 10, 1, 2, 3, DateTimeKind.Utc)
                .AddMilliseconds(456);
            s.Serialize(sw, new Entity
            {
                Data = "some\r\nline", Timestamp =
                    ts
            });
            sw.ToString().Should()
                .Be(@"{'$type':'entity','Data':'some\r\nline','number':null,'Timestamp':'2019-08-10T01:02:03.456Z'}"
                    .Replace('\'', '"'));
            var t = s.Deserialize(new JsonTextReader(new StringReader(sw.ToString())))
                as Entity;
            t.Should().NotBeNull();
            t.Data.Should().Be("some\r\nline");
            t.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
            t.Timestamp.Should().Be(ts);
        }

        [Fact]
        public void Should_serialize_TimeSpan()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TimeSpanConverter());
            var text = JsonConvert
                .SerializeObject(new Entity { Duration = TimeSpan.FromSeconds(90) },
                    settings);
            text.Should().Contain("Duration\":\"PT1M30S");
            var obj = JsonConvert.DeserializeObject<Entity>(text, settings);
            obj.Duration.TotalSeconds.Should().Be(90);
        }

        private class Entity
        {
            public string Data { get; set; }
            public int DefaultInt { get; set; }

            [JsonProperty("number", DefaultValueHandling = DefaultValueHandling.Include)]
            public int? ExplicitInt { get; set; }

            public DateTime Timestamp { get; set; }
            public TimeSpan Duration { get; set; }
        }
    }
}