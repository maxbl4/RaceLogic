using System;
using System.Linq;
using FluentAssertions;
using LiteDB;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Tests.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class EntityIdTests
    {
        private readonly string dbFileName;
        public EntityIdTests(ITestOutputHelper outputHelper)
        {
            dbFileName = outputHelper.GetEmptyLiteDbForTest();
        }
        
        [Fact]
        public void Should_support_custom_ids()
        {
            var guid1 = new Guid("FA0551EE-4BB4-491E-A46F-76E38952F97C");
            var rider = new Rider {Id = guid1, ClassId = guid1};
            Guid guid2 = rider.Id;
            guid1.Should().Be(guid2);
            rider.Id.Should().Be((Id<Rider>)guid1);
            rider.Id.Should().NotBe(rider.ClassId);
            var @class = new Class {Id = Id<Class>.NewId()};
            @class.Id.Value.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Should_serialize_and_deserialize_to_litedb()
        {
            using var repo = new LiteRepository(dbFileName);
            var cls = new Class{ Id = Id<Class>.NewId(), Name = "Class1" }; 
            var rider = new Rider{ Id = Id<Rider>.NewId(), Name = "Rider1", ClassId = cls.Id };
            repo.Insert(cls);
            repo.Upsert(rider);
            repo.Upsert(rider);
            repo.Query<Rider>().Count().Should().Be(1);

            var persistedRider = repo.Query<Rider>().Where(x => x.Id == rider.Id).First();
            persistedRider.Name.Should().Be("Rider1");
            var persistedClass = repo.Query<Class>().Where(x => x.Id == persistedRider.ClassId).First();
            persistedClass.Name.Should().Be("Class1");
        }
        
        [Fact]
        public void Should_generate_default_value_for_id()
        {
            using var repo = new LiteRepository(dbFileName);
            repo.Insert(new Rider{ Name = "Rider1" });
            repo.Insert(new Rider{ Name = "Rider2" });
            repo.Query<Rider>().Count().Should().Be(2);
        }
        
        [Fact]
        public void Should_use_custom_id_serializer()
        {
            BsonMapper.Global.RegisterType(x => x.Value.ToString("N"), x => new Id<Rider>(Guid.Parse(x)));
            using var repo = new LiteRepository(dbFileName);
            repo.Insert(new Rider{ Name = "Rider1" });
            repo.Insert(new Rider{ Name = "Rider2" });
            repo.Query<Rider>().Count().Should().Be(2);
            var rider = repo.Database.GetCollection("Rider").FindAll().First();
            rider["_id"].Type.Should().Be(BsonType.String);
            Guid.Parse(rider["_id"]).Should().NotBeEmpty();
        }
        
        class Rider
        {
            public Id<Rider> Id { get; set; } = Id<Rider>.NewId();
            public string Name { get; set; }
            public Id<Class> ClassId { get; set; }
        }
        
        class Class
        {
            public Id<Class> Id { get; set; }
            public string Name { get; set; }
        }
    }
}