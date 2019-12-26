using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LiteDB;
using maxbl4.Race.Tests.Extensions;
using Shouldly;
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
            var id = new Guid("D390C953-3F55-4558-8C0B-77FBAE798C9D");
            var e = new Entity{Id = id, Some = "abc", Int = 123};
            
            var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", e);
            
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldBe(new Uri($"{DataUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9dg"));
            (await http.GetBsonAsync<Entity>(response.Headers.Location)).ShouldBe(e);

            e.Some = "eee";
            response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", e);
            
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldBe(new Uri($"{DataUri}/store/Entity/single/d390c9533f5545588c0b77fbae798c9dg"));
            (await http.GetBsonAsync<Entity>(response.Headers.Location)).ShouldBe(e);
        }
        
        [Fact]
        public async Task Should_store_entity_without_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var e = new Entity{Some = "abc", Int = 123};
            
            var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", e);
            
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var e2 = await http.GetBsonAsync<Entity>(response.Headers.Location);
            e2.Id.ShouldNotBe(Guid.Empty);
            e2.Some.ShouldBe("abc");
        }
        
        [Fact]
        public async Task Should_honor_id_in_uri()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single/5", new EntityInt{ Id = 2, Some = "2"});
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldBe(new Uri($"{DataUri}/store/Entity/single/5"));
            var e = await http.GetBsonAsync<EntityInt>(response.Headers.Location);
            e.Some.ShouldBe("2");
        }
        
        [Fact]
        public async Task Should_return_404_on_invalid_key()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var response = await http.GetAsync($"{DataUri}/store/Entity/single/5");
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task Should_store_bson_without_id()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var e = new BsonDocument{["abc"] = "def"};
            
            var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", e);
            
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            var e2 = await http.GetBsonAsync<BsonDocument>(response.Headers.Location);
            e2["_id"].Type.ShouldBe(BsonType.Guid);
            e2["_id"].AsGuid.ShouldNotBe(Guid.Empty);
            e2["abc"].AsString.ShouldBe("def");
        }
        
        [Fact]
        public async Task Should_store_entities_with_numeric_ids()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var eInt = new EntityInt{Some = "int"};
            var responseInt = await http.PostBsonAsync($"{DataUri}/store/Some1/single", eInt);
            responseInt.StatusCode.ShouldBe(HttpStatusCode.Created);
            responseInt.Headers.Location.ShouldBe(new Uri($"{DataUri}/store/Some1/single/1"));
            var respInt = await http.GetBsonAsync<EntityInt>(responseInt.Headers.Location);
            respInt.Id.ShouldBe(1);
            respInt.Some.ShouldBe("int");
            
            var eLong = new EntityLong{Some = "long"};
            var responseLong = await http.PostBsonAsync($"{DataUri}/store/Some2/single", eLong);
            responseLong.StatusCode.ShouldBe(HttpStatusCode.Created);
            responseLong.Headers.Location.ShouldBe(new Uri($"{DataUri}/store/Some2/single/1L"));
            var respLong = await http.GetBsonAsync<EntityLong>(responseLong.Headers.Location);
            respLong.Id.ShouldBe(1L);
            respLong.Some.ShouldBe("long");
        }
        
        
        [Fact]
        public async Task Should_search_with_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{DataUri}/store/Entity/search?where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.Count.ShouldBe(4);
            e.ShouldContain(x => x.Int == 4);
            e.ShouldContain(x => x.Int == 5);
            e.ShouldContain(x => x.Int == 6);
            e.ShouldContain(x => x.Int == 7);
        }
        
        [Fact]
        public async Task Should_search_with_limit()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{DataUri}/store/Entity/search?limit=2&where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.Count.ShouldBe(2);
        }
        
        [Fact]
        public async Task Should_search_with_order()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{DataUri}/store/Entity/search?order=-Int");
            e.Count.ShouldBe(10);
            for (var i = 0; i < 10; i++)
            {
                e[i].Int.ShouldBe(9 - i);
            }
        }
        
        [Fact]
        public async Task Should_search_with_empty_where_and_have_default_limit_of_50()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 60; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetBsonAsync<List<Entity>>($"{DataUri}/store/Entity/search");
            e.Count.ShouldBe(50);
        }
        
        [Fact]
        public async Task Should_get_count_with_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetStringAsync($"{DataUri}/store/Entity/count?where=" + UrlEncoder.Default.Encode("Int > 3 and Int <= 7"));
            e.ShouldBe("4");
        }
        
        [Fact]
        public async Task Should_get_count_without_where()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            for (var i = 0; i < 10; i++)
            {
                var response = await http.PostBsonAsync($"{DataUri}/store/Entity/single", new Entity{Int = i});
                response.EnsureSuccessStatusCode();
            }
            
            var e = await http.GetStringAsync($"{DataUri}/store/Entity/count");
            e.ShouldBe("10");
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
                if (obj.GetType() != this.GetType()) return false;
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

        class EntityInt
        {
            public int Id { get; set; }
            public string Some { get; set; }
        }
        
        class EntityLong
        {
            public long Id { get; set; }
            public string Some { get; set; }
        }
    }
}