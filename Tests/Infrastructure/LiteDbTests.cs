using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using LiteDB;
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
            e.Data.Should().Be("123");
            e.Id.Should().NotBe(Guid.Empty);
        }
        
        [Fact]
        public void Should_query_by_literal_guid()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Id = g, Data = "123"});
            var e = repo.Query<Entity>().Where($"_id = GUID('{g}')").First();
            e.Data.Should().Be("123");
        }
        
        [Fact]
        public void Should_query_bson_document()
        {
            var g = new Guid("CFF4EF1C-A0DB-4F0C-B1DE-FCFEC7028CFF");
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Id = g, Data = "123"});
            var doc = repo.Query<BsonDocument>("Entity").Where($"_id = GUID('{g}')").First();
            doc["Data"].AsString.Should().Be("123");
        }
        
        [Fact]
        public void Should_filter_by_literal_date()
        {
            using var repo = new LiteRepository(dbFile);
            repo.Insert(new Entity { Data = "555"});
            var e = repo.Query<Entity>().Where($"Date < DATE('{DateTime.UtcNow:u}')").First();
            e.Data.Should().Be("555");
        }
        
        [Fact]
        public void Should_insert_new_bson_document_with_guid_id()
        {
            using var repo = new LiteRepository(dbFile);
            var collection = repo.Database.GetCollection("Entity", BsonAutoId.Guid);
            var doc = new BsonDocument {["Data"] = "666"};
            collection.Upsert(doc);
            repo.Insert(new Entity { Data = "555"});
            var docs = repo.Query<Entity>().ToList();
            docs.Should().OnlyContain(x => x.Id != Guid.Empty);
        }

        [Fact]
        public void Should_store_documents_with_int_key_in_sorted_order()
        {
            using (var repo = new LiteRepository(dbFile))
            {
                repo.Insert(new EntityInt {Id = 5,});
                repo.Insert(new EntityInt {Id = 4,});
                repo.Insert(new EntityInt {Id = 7,});
                repo.Insert(new EntityInt {Id = 3,});
            }
            
            using (var repo = new LiteRepository(dbFile))
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
            using (var repo = new LiteRepository(dbFile))
            {
                repo.Insert(new Entity {Id = new Guid("5B8E621A-CBC4-4052-B58C-4ACAFC3A6864")});
                repo.Insert(new Entity {Id = new Guid("1B8E621A-CBC4-4052-B58C-4ACAFC3A6864")});
            }
            
            using (var repo = new LiteRepository(dbFile))
            {
                var docs = repo.Query<Entity>().ToList();
                docs[0].Id.Should().Be(new Guid("1B8E621A-CBC4-4052-B58C-4ACAFC3A6864"));
            }
        }
        
        [Fact(Skip = "Long running")]
        public void Should_insert_many_documents()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            var token = cts.Token;
            using (var repo = new LiteRepository(dbFile))
            {
                for (var i = 0; i < 10000000; i++)
                {
                    repo.Insert(new Entity {Data = i.ToString()});
                    if (token.IsCancellationRequested)
                        break;
                }
            }
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