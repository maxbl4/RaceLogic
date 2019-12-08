using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Tag = maxbl4.Race.CheckpointService.Model.Tag;

namespace maxbl4.Race.Tests.CheckpointService.Controllers
{
    public class TagControllerTests : IntegrationTestBase
    {
        public TagControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task Should_return_stored_tags()
        {
            var ts = DateTime.UtcNow;
            WithStorageService(storageService =>
            {
                storageService.AppendTag(new Tag{ TagId = "stored1", DiscoveryTime = ts});
                storageService.AppendTag(new Tag{ TagId = "stored2", DiscoveryTime = ts.AddSeconds(100)});
                for (var i = 0; i < 98; i++)
                {
                    storageService.AppendTag(new Tag{ TagId = "tag", DiscoveryTime = ts.AddSeconds(i + 105)});
                }
            });
            
            using var svc = CreateCheckpointService();
            var client = new HttpClient();
            var tags = await client.GetAsync<List<Tag>>($"{CheckpointsUri}/tag");
            
            tags.ShouldNotBeNull();
            tags.Count.ShouldBe(100);
            tags[99].TagId.ShouldBe("stored1");
            tags[98].TagId.ShouldBe("stored2");
            
            tags = await client.GetAsync<List<Tag>>($"{CheckpointsUri}/tag?count=2&start={ts.AddSeconds(50):u}&end={ts.AddSeconds(106):u}");
            tags.Count.ShouldBe(2);
            tags[1].TagId.ShouldBe("stored2");
            tags[0].TagId.ShouldBe("tag");
        }
        
        [Fact]
        public async Task Should_remove_tags()
        {
            var now = DateTime.UtcNow;
            WithStorageService(storageService =>
                {
                    storageService.AppendTag(new Tag{TagId = "1"});
                });
            
            using var svc = CreateCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.DeleteAsync($"{CheckpointsUri}/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(1);
            
            response = await cli.DeleteAsync($"{CheckpointsUri}/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(0);
            var tags = await cli.GetAsync<List<Tag>>($"{CheckpointsUri}/tag");
            tags.Count.ShouldBe(0);
        }
        
        [Fact]
        public async Task Should_return_version()
        {
            using var svc = CreateCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.GetStringAsync($"{CheckpointsUri}/version");
            response.ShouldBe("1.0.0.0");
        }
    }
}