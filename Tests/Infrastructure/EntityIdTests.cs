using System;
using System.Linq;
using FluentAssertions;
using LiteDB;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.Extensions;
using maxbl4.Race.Tests.Extensions;
using Newtonsoft.Json;
using SequentialGuid;
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
            rider.Id.Should().Be(guid1);
            var @class = new Class {Id = SequentialGuidGenerator.Instance.NewGuid()};
            @class.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Should_serialize_and_deserialize_to_litedb()
        {
            using var repo = new LiteRepository(dbFileName).WithUtcDate();
            var cls = new Class {Id = SequentialGuidGenerator.Instance.NewGuid(), Name = "Class1"};
            var rider = new Rider {Id = SequentialGuidGenerator.Instance.NewGuid(), Name = "Rider1", ClassId = cls.Id};
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
            using var repo = new LiteRepository(dbFileName).WithUtcDate();
            repo.Insert(new Rider {Name = "Rider1"});
            repo.Insert(new Rider {Name = "Rider2"});
            repo.Query<Rider>().Count().Should().Be(2);
        }

        [Fact]
        public void Should_use_custom_id_serializer()
        {
            BsonMapper.Global.RegisterType(x => x.ToString("N"), x => Guid.Parse(x));
            using var repo = new LiteRepository(dbFileName).WithUtcDate();
            repo.Insert(new Rider {Name = "Rider1"});
            repo.Insert(new Rider {Name = "Rider2"});
            repo.Query<Rider>().Count().Should().Be(2);
            var rider = repo.Database.GetCollection("Rider").FindAll().First();
            rider["_id"].Type.Should().Be(BsonType.String);
            Guid.Parse(rider["_id"]).Should().NotBeEmpty();
        }

        [Fact]
        public void Should_use_dynamically_registered_id_mappings()
        {
            BsonMapper.Global.RegisterIdBsonMappers(GetType().Assembly);
            using var repo = new LiteRepository(dbFileName).WithUtcDate();
            var r1 = new Rider {Name = "Rider1"};
            repo.Insert(r1);
            repo.Insert(new Rider {Name = "Rider2"});
            repo.Query<Rider>().Count().Should().Be(2);
            var rider = repo.Database.GetCollection("Rider").FindAll().First();
            rider["_id"].Type.Should().Be(BsonType.String);
            Guid.Parse(rider["_id"]).Should().NotBeEmpty();
            repo.Query<Rider>().Where(x => x.Id == r1.Id).First().Should().NotBeNull();
            repo.Delete<Rider>(r1.Id).Should().BeTrue();
        }

        [Fact]
        public void Should_convert_to_json()
        {
            var id = new Guid("cbc9da11e6214d6196a7fb6ba9ddcba4");
            var rider = new Rider {Id = id, Name = "Rider1"};
            JsonConvert.SerializeObject(rider).Should()
                .Be(
                    @"{'Name':'Rider1','ClassId':'00000000000000000000000000000000','Id':'cbc9da11e6214d6196a7fb6ba9ddcba4'}"
                        .Replace('\'', '"'));
        }

        [Fact]
        public void New_ids_should_be_sequential()
        {
            var id = Guid.Empty;
            for (var i = 0; i < 100; i++)
            {
                var next = SequentialGuidGenerator.Instance.NewGuid();
                next.CompareTo(id).Should().BePositive();
                id = next;
            }
        }

        private class Rider : IHasId<Rider>
        {
            public string Name { get; set; }
            public Guid ClassId { get; set; }
            public Guid Id { get; set; } = SequentialGuidGenerator.Instance.NewGuid();
        }

        private class Class : IHasId<Class>
        {
            public string Name { get; set; }
            public Guid Id { get; set; }
        }
    }
}