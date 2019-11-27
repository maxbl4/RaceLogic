using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidCheckpointService.Model;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Controllers
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
            });
            
            using var svc = CreateRfidCheckpointService();
            var client = new HttpClient();
            var tags = await client.GetAsync<List<Tag>>("http://localhost:5000/tag");
            
            tags.ShouldNotBeNull();
            tags.Count.ShouldBe(2);
            tags.ShouldContain(x => x.TagId == "stored1");
            tags.ShouldContain(x => x.TagId == "stored2");
        }
        
        [Fact]
        public async Task Should_remove_tags()
        {
            var now = DateTime.UtcNow;
            WithStorageService(storageService =>
                {
                    storageService.AppendTag(new Tag{TagId = "1"});
                });
            
            using var svc = CreateRfidCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.DeleteAsync("http://localhost:5000/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(1);
            
            response = await cli.DeleteAsync($"http://localhost:5000/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(0);
            var tags = await cli.GetAsync<List<Tag>>("http://localhost:5000/tag");
            tags.Count.ShouldBe(0);
        }
    }
}