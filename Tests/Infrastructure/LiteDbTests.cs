using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using LiteDB;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.Extensions;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class LiteDbTests
    {
        private const string dbFile = nameof(LiteDbTests) + ".litedb";

        public LiteDbTests()
        {
            if (File.Exists(dbFile))
                File.Delete(dbFile);
        }

        [Fact]
        public void Should_generate_guid_id()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            var entity = new Entity {Data = "123"};
            repo.Insert(entity);
            var e = repo.Query<Entity>().Where(x => x.Id == entity.Id).First();
            e.Data.Should().Be("123");
            e.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Should_generate_long_id()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            var entity = new EntityLong {Data = "123"};
            repo.Upsert(entity);
            var e = repo.Query<EntityLong>().Where(x => x.Id == entity.Id).First();
            e.Data.Should().Be("123");
            e.Id.Should().NotBe(0);
        }

        [Fact]
        public void Should_generate_long_id_for_bson_document()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            var doc = new BsonDocument {["Some"] = "123", ["_id"] = 0L};
            var col = repo.Database.GetCollection("some", DataServiceRepository.GetAutoId(doc, out var isDefault));
            if (isDefault)
                doc.Remove("_id");
            col.Upsert(doc);
            var id = doc["_id"].AsInt64;
            id.Should().NotBe(0);
            var e = col.FindById(id);
            e["Some"].Should().Be("123");
        }

        [Fact]
        public void Should_query_by_literal_guid()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            repo.Insert(new Entity {Id = g, Data = "123"});
            var e = repo.Query<Entity>().Where($"_id = GUID('{g}')").First();
            e.Data.Should().Be("123");
        }

        [Fact]
        public void Should_query_bson_document()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            repo.Insert(new Entity {Id = g, Data = "123"});
            var doc = repo.Query<BsonDocument>("Entity").Where($"_id = GUID('{g}')").First();
            doc["Data"].AsString.Should().Be("123");
        }

        [Fact]
        public void Should_filter_by_literal_date()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            repo.Insert(new Entity {Data = "555"});
            var e = repo.Query<Entity>().Where($"Date < DATE('{DateTime.UtcNow:u}')").First();
            e.Data.Should().Be("555");
        }

        [Fact]
        public void Should_insert_new_bson_document_with_guid_id()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            var collection = repo.Database.GetCollection("Entity", BsonAutoId.Guid);
            var doc = new BsonDocument {["Data"] = "666"};
            collection.Upsert(doc);
            repo.Insert(new Entity {Data = "555"});
            var docs = repo.Query<Entity>().ToList();
            docs.Should().OnlyContain(x => x.Id != Guid.Empty);
        }

        [Fact]
        public void Should_store_documents_with_int_key_in_sorted_order()
        {
            using (var repo = new LiteRepository(dbFile).WithUtcDate())
            {
                repo.Insert(new EntityInt {Id = 5});
                repo.Insert(new EntityInt {Id = 4});
                repo.Insert(new EntityInt {Id = 7});
                repo.Insert(new EntityInt {Id = 3});
            }

            using (var repo = new LiteRepository(dbFile).WithUtcDate())
            {
                var docs = repo.Query<EntityInt>().ToList();
                docs[0].Id.Should().Be(3);
                docs[1].Id.Should().Be(4);
                docs[2].Id.Should().Be(5);
                docs[3].Id.Should().Be(7);
            }
        }

        [Fact]
        public void Should_store_documents_with_guid_key_in_sorted_order()
        {
            using (var repo = new LiteRepository(dbFile).WithUtcDate())
            {
                repo.Insert(new Entity {Id = new Guid("5B8E621A-CBC4-4052-B58C-4ACAFC3A6864")});
                repo.Insert(new Entity {Id = new Guid("1B8E621A-CBC4-4052-B58C-4ACAFC3A6864")});
            }

            using (var repo = new LiteRepository(dbFile).WithUtcDate())
            {
                var docs = repo.Query<Entity>().ToList();
                docs[0].Id.Should().Be(new Guid("1B8E621A-CBC4-4052-B58C-4ACAFC3A6864"));
            }
        }

        [Theory]
        [InlineData(1000)]
        public void Should_insert_many_documents(int count)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            var token = cts.Token;
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            for (var i = 0; i < count; i++)
            {
                repo.Insert(new Entity {Data = i.ToString()});
                if (token.IsCancellationRequested)
                    break;
            }
        }

        [Fact]
        public void Read_bson_with_more_fields_then_entity()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();
            repo.Insert(new BigEntity{Field1 = "111", Field2 = "222"}, "col");
            var e = repo.Query<SmallEntity>("col").First();
            e.Field1.Should().Be("111");
            repo.Update(e, "col");
            var e2 = repo.Query<BigEntity>("col").First();
            e2.Field1.Should().Be("111");
            e2.Field2.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void Read_untyped_object()
        {
            using var repo = new LiteRepository(dbFile).WithUtcDate();

            var custom = new SmallEntity {Id = Id<SmallEntity>.NewId(), Field1 = "ffff"};
            repo.Insert(new BigEntity{Field1 = "111", Field2 = "222", Custom = custom});
            var e = repo.Query<BigEntity>().First();
            var custom2 = e.Custom.Should().BeOfType<SmallEntity>().Subject;
            custom2.Id.Should().Be(custom.Id);
            custom2.Field1.Should().Be(custom.Field1);
        }

        private class BigEntity: IHasId<BigEntity>
        {
            public Id<BigEntity> Id { get; set; }
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public object Custom { get; set; }
        }
        
        private class SmallEntity: IHasId<SmallEntity>
        {
            public Id<SmallEntity> Id { get; set; }
            public string Field1 { get; set; }
        }

        private class Entity
        {
            public Guid Id { get; set; }
            public string Data { get; set; }
            public DateTime Date { get; set; } = DateTime.UtcNow.AddSeconds(-10);
        }

        private class EntityInt
        {
            public int Id { get; set; }
            public string Data { get; set; }
        }

        private class EntityLong
        {
            public long Id { get; set; }
            public string Data { get; set; }
        }
    }
}