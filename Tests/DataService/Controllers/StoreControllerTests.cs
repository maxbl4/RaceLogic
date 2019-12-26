using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task Should_upsert_bson_document()
        {
            using var svc = CreateDataService();
            var http = new HttpClient();
            var id = new Guid("D390C953-3F55-4558-8C0B-77FBAE798C9D");
            var doc = new BsonDocument {["_id"] = id, ["some"] = "abc", ["int"] = 123};
            var json = JsonSerializer.Serialize(doc);
            var response = await http.PostAsync($"{DataUri}/store/some/upsert", new StringContent(json, Encoding.UTF8, "application/json"));
            response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

            var storageService = svc.ServiceProvider.GetService<maxbl4.Race.DataService.Services.StorageService>();
            var docs = storageService.Search("some", $"_id = GUID('{id}')", 10).ToList();
            docs.Count.ShouldBe(1);
            docs[0]["_id"].AsGuid.ShouldBe(id);
            docs[0]["some"].AsString.ShouldBe("abc");
            docs[0]["int"].AsInt32.ShouldBe(123);
        }
    }
}