using System;
using LiteDB;
using maxbl4.Race.DataService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.DataService.Services
{
    public class BsonIdUrlEncoderTests
    {
        [Fact]
        public void Should_support_int()
        {
            var id = BsonIdUrlEncoder.Decode("123");
            id.Type.ShouldBe(BsonType.Int32);
            id.AsInt32.ShouldBe(123);
            BsonIdUrlEncoder.Encode(id).ShouldBe("123");

            Assert.Throws<ArgumentException>(() => BsonIdUrlEncoder.Decode("123e"));
        }
        
        [Fact]
        public void Should_support_long()
        {
            var id = BsonIdUrlEncoder.Decode("123L");
            id.Type.ShouldBe(BsonType.Int64);
            id.AsInt64.ShouldBe(123L);
            BsonIdUrlEncoder.Encode(id).ShouldBe("123L");
            BsonIdUrlEncoder.Decode("123l").ShouldBe(id);
        }
        
        [Fact]
        public void Should_support_guid()
        {
            var guid = new Guid("7E360B14-B0DF-47FF-A491-518A3E11C0A2");
            var id = BsonIdUrlEncoder.Decode(guid + "g");
            id.Type.ShouldBe(BsonType.Guid);
            id.AsGuid.ShouldBe(guid);
            BsonIdUrlEncoder.Encode(id).ShouldBe(guid.ToString("N") + "g");
            BsonIdUrlEncoder.Decode(guid.ToString("N") + "G").ShouldBe(id);
        }
        
        [Fact]
        public void Should_support_objectId()
        {
            var oid = ObjectId.NewObjectId();
            var id = BsonIdUrlEncoder.Decode(oid + "o");
            id.Type.ShouldBe(BsonType.ObjectId);
            id.AsObjectId.ShouldBe(oid);
            BsonIdUrlEncoder.Encode(id).ShouldBe(oid + "o");
            BsonIdUrlEncoder.Decode(oid + "O").ShouldBe(id);
        }
    }
}