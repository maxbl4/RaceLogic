using System;
using FluentAssertions;
using LiteDB;
using maxbl4.Race.DataService.Services;
using Xunit;

namespace maxbl4.Race.Tests.DataService.Services
{
    public class BsonIdUrlEncoderTests
    {
        [Fact]
        public void Should_support_string()
        {
            var id = BsonIdUrlEncoder.Decode("123");
            id.Type.Should().Be(BsonType.String);
            BsonIdUrlEncoder.Encode(id).Should().Be("123");
            BsonIdUrlEncoder.Decode("123e").Type.Should().Be(BsonType.String);
        }

        [Fact]
        public void Should_support_long()
        {
            var id = BsonIdUrlEncoder.Decode("123L");
            id.Type.Should().Be(BsonType.Int64);
            id.AsInt64.Should().Be(123L);
            BsonIdUrlEncoder.Encode(id).Should().Be("123L");
            BsonIdUrlEncoder.Decode("123l").Should().Be(id);
        }

        [Fact]
        public void Should_support_guid()
        {
            var guid = new Guid("7E360B14-B0DF-47FF-A491-518A3E11C0A2");
            var id = BsonIdUrlEncoder.Decode(guid + "g");
            id.Type.Should().Be(BsonType.Guid);
            id.AsGuid.Should().Be(guid);
            BsonIdUrlEncoder.Encode(id).Should().Be(guid.ToString("N") + "g");
            BsonIdUrlEncoder.Decode(guid.ToString("N") + "G").Should().Be(id);
        }

        [Fact]
        public void Should_support_objectId()
        {
            var oid = ObjectId.NewObjectId();
            var id = BsonIdUrlEncoder.Decode(oid + "o");
            id.Type.Should().Be(BsonType.ObjectId);
            id.AsObjectId.Should().Be(oid);
            BsonIdUrlEncoder.Encode(id).Should().Be(oid + "o");
            BsonIdUrlEncoder.Decode(oid + "O").Should().Be(id);
        }
    }
}