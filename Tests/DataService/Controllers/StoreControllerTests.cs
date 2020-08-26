using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using LiteDB;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Tests.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.DataService.Controllers
{
    public class StoreControllerTests : IntegrationTestBase
    {
        public StoreControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task Should_store_entity_with_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var id = new Guid("d390c9533f5545588c0b77fbae798c9d");
            var e = new Entity{Id = id, Some = "abc", Int = 123};
            
            var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9dg"));
            (await http.GetBsonAsync<Entity>(response.Headers.Location)).Should().Be(e);

            e.Some = "eee";
            response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9dg"));
            (await http.GetBsonAsync<Entity>(response.Headers.Location)).Should().Be(e);
        }
        
        [Fact]
        public async Task Should_store_entity_with_value_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var id = new Guid("d390c9533f5545588c0b77fbae798c9d");
            var e = new EntityWithId{Id = id, Some = "abc", Int = 123};
            
            var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9d"));
            (await http.GetBsonAsync<EntityWithId>(response.Headers.Location)).Should().Be(e);

            e.Some = "eee";
            response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9d"));
            (await http.GetBsonAsync<EntityWithId>(response.Headers.Location)).Should().Be(e);
        }
        
        
        [Fact]
        public async Task Should_store_entity_without_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var e = new Entity{Some = "abc", Int = 123};
            
            var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var e2 = await http.GetBsonAsync<Entity>(response.Headers.Location);
            e2.Id.Should().NotBe(Guid.Empty);
            e2.Some.Should().Be("abc");
        }
        
        [Fact]
        public async Task Should_honor_id_in_uri()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single/5", new EntityString{ Id = "2", Some = "2"});
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Entity/single/5"));
            var e = await http.GetBsonAsync<EntityString>(response.Headers.Location);
            e.Some.Should().Be("2");
        }
        
        [Fact]
        public async Task Should_return_404_on_invalid_key()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var response = await http.GetAsync($"{svc.ListenUri}/store/Entity/single/5");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task Should_store_bson_without_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var e = new BsonDocument{["abc"] = "def"};
            
            var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", e);
            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var e2 = await http.GetBsonAsync<BsonDocument>(response.Headers.Location);
            e2["_id"].Type.Should().Be(BsonType.Guid);
            e2["_id"].AsGuid.Should().NotBe(Guid.Empty);
            e2["abc"].AsString.Should().Be("def");
        }
        
        [Fact]
        public async Task Should_store_entities_with_numeric_ids()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();

            var eLong = new EntityLong{Some = "long"};
            var responseLong = await http.PostBsonAsync($"{svc.ListenUri}/store/Some2/single", eLong);
            responseLong.StatusCode.Should().Be(HttpStatusCode.Created);
            responseLong.Headers.Location.Should().Be(new Uri($"{svc.ListenUri}/store/Some2/single/1L"));
            var respLong = await http.GetBsonAsync<EntityLong>(responseLong.Headers.Location);
            respLong.Id.Should().Be(1L);
            respLong.Some.Should().Be("long");
        }
        
        [Fact]
        public async Task Should_delete_by_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 3; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new EntityLong{Some = i.ToString()});
                response.EnsureSuccessStatusCode();
            }

            (await http.DeleteAsync($"{svc.ListenUri}/store/Entity/single/1L")).EnsureSuccessStatusCode();
            (await http.DeleteAsync($"{svc.ListenUri}/store/Entity/single/2L")).EnsureSuccessStatusCode();
            
            var e = await http.GetBsonAsync<List<EntityLong>>($"{svc.ListenUri}/store/Entity/search");
            e.Count.Should().Be(1);
            e.Should().Contain(x => x.Some == "2");
        }
        
        [Fact]
        public async Task Should_search_with_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{svc.ListenUri}/store/Entity/search?where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.Count.Should().Be(4);
            e.Should().Contain(x => x.Int == 4);
            e.Should().Contain(x => x.Int == 5);
            e.Should().Contain(x => x.Int == 6);
            e.Should().Contain(x => x.Int == 7);
        }
        
        [Fact]
        public async Task Should_search_with_limit()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{svc.ListenUri}/store/Entity/search?limit=2&where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.Count.Should().Be(2);
        }
        
        [Fact]
        public async Task Should_search_with_order()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{svc.ListenUri}/store/Entity/search?order=-Int");
            e.Count.Should().Be(10);
            for (var i = 0; i < 10; i++)
            {
                e[i].Int.Should().Be(9 - i);
            }
        }
        
        [Fact]
        public async Task Should_search_with_empty_where_and_have_default_limit_of_50()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 60; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{svc.ListenUri}/store/Entity/search");
            e.Count.Should().Be(50);
        }
        
        [Fact]
        public async Task Should_get_count_with_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetStringAsync($"{svc.ListenUri}/store/Entity/count?where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.Should().Be("4");
        }
        
        [Fact]
        public async Task Should_get_count_without_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{svc.ListenUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetStringAsync($"{svc.ListenUri}/store/Entity/count");
            e.Should().Be("10");
        }

        class Entity
        {
            protected bool Equals(Entity other)
            {
                return Id.Equals(other.Id) && Some == other.Some && Int == other.Int;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Entity) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Some, Int);
            }

            public Guid Id { get; set; }
            public string Some { get; set; }
            public string Some2 { get; set; }
            public string Some3 { get; set; }
            public int Int { get; set; }
        }
        
        class EntityWithId: IHasId<EntityWithId>
        {
            protected bool Equals(EntityWithId other)
            {
                return Id.Equals(other.Id) && Some == other.Some && Int == other.Int;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((EntityWithId) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Some, Int);
            }

            public Id<EntityWithId> Id { get; set; } = Id<EntityWithId>.NewId();
            public string Some { get; set; }
            public string Some2 { get; set; }
            public string Some3 { get; set; }
            public int Int { get; set; }
        }

        class EntityString
        {
            public string Id { get; set; }
            public string Some { get; set; }
        }
        
        class EntityLong
        {
            public long Id { get; set; }
            public string Some { get; set; }
        }
    }
}