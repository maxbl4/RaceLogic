using System;
using System.IO;
using AutoMapper.Mappers;
using LiteDB;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class LiteDbTests
    {
        const string dbFile = nameof(LiteDbTests) + ".litedb";
        
        public LiteDbTests()
        {
            if (File.Exists(dbFile))
                File.Delete(dbFile);
        }
        
        [Fact]
        public void Should_generate_guid_id()
        {
            using var repo = new LiteRepository(dbFile);
            var entity = new Entity {Data = "123"};
            repo.Insert(entity);
            var e = repo.Query<Entity>().Where(x => x.Id == entity.Id).First();
            e.Data.ShouldBe("123");
            e.Id.ShouldNotBe(Guid.Empty);
        }
        
        [Fact]
        public void Should_query_by_literal_guid()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Id = g, Data = "123"});
            var e = repo.Query<Entity>().Where($"_id = GUID('{g}')").First();
            e.Data.ShouldBe("123");
        }
        
        [Fact]
        public void Should_query_bson_document()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Id = g, Data = "123"});
            var doc = repo.Query<BsonDocument>("Entity").Where($"_id = GUID('{g}')").First();
            doc["Data"].AsString.ShouldBe("123");
        }
        
        [Fact]
        public void Should_filter_by_literal_date()
        {
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Data = "555"});
            var e = repo.Query<Entity>().Where($"Date < DATE('{DateTime.UtcNow:u}')").First();
            e.Data.ShouldBe("555");
        }

        class Entity
        {
            public Guid Id { get; set; }
            public string Data { get; set; }
            public DateTime Date { get; set; } = DateTime.UtcNow.AddSeconds(-10);
        }
        
        class EntityInt
        {
            public int Id { get; set; }
            public string Data { get; set; }
        }
        
        class EntityLong
        {
            public long Id { get; set; }
            public string Data { get; set; }
        }
    }
}