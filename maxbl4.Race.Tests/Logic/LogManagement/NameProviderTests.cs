using System;
using System.Linq;
using maxbl4.Race.Logic.LogManagement;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.Logic.LogManagement
{
    public class NameProviderTests
    {
        private readonly DateTime sampleDate = new DateTime(2019, 07, 12, 14, 35, 57, DateTimeKind.Utc);
        private const string sampleDateString = "2019-07-12_14-35-57Z";
        private const string sampleFilename = sampleDateString + "_some-name";
        [Fact]
        public void Should_parse_name()
        {
            var (success, logName) = new NameProvider().ParseName(sampleFilename);
            success.ShouldBeTrue();
            logName.Timestamp.ShouldBe(sampleDate);
            logName.Name.ShouldBe("some-name");
            logName.Filename.ShouldBe(sampleFilename);
        }

        [Fact]
        public void Should_serialize_name()
        {
            var filename = new NameProvider().SerializeName(new LogName(sampleDate, "some-name", null));
            filename.ShouldBe(sampleFilename);
        }

        [Fact]
        public void Should_encode_path_string()
        {
            var encoded = new NameProvider().PathEncode("+\t");
            encoded.ShouldBe(@"+002b+0009");
        }

        [Fact]
        public void Should_decode_path_string()
        {
            var decoded = new NameProvider().PathDecode(@"+002b+0009");
            decoded.ShouldBe("+\t");
        }

        [Fact]
        public void Should_encode_decode_all_chars_string()
        {
            var str = new string(Enumerable.Range(0, ushort.MaxValue + 1)
                .Select(x => (char)x).ToArray());
            var encoded = new NameProvider().PathEncode(str);
            var decoded = new NameProvider().PathDecode(encoded);
            decoded.ShouldBe(str);
        }
    }
}