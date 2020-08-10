﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Race.Services.CheckpointService.Model;
using Xunit;
using Xunit.Abstractions;

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
            WithCheckpointStorageService(storageService =>
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
            var tags = await client.GetAsync<List<Tag>>($"{svc.ListenUri}/tag");
            
            tags.Should().NotBeNull();
            tags.Count.Should().Be(100);
            tags[99].TagId.Should().Be("stored1");
            tags[98].TagId.Should().Be("stored2");
            
            tags = await client.GetAsync<List<Tag>>($"{svc.ListenUri}/tag?count=2&start={ts.AddSeconds(50):u}&end={ts.AddSeconds(106):u}");
            tags.Count.Should().Be(2);
            tags[1].TagId.Should().Be("stored2");
            tags[0].TagId.Should().Be("tag");
        }
        
        [Fact]
        public async Task Should_remove_tags()
        {
            var now = DateTime.UtcNow;
            WithCheckpointStorageService(storageService =>
                {
                    storageService.AppendTag(new Tag{TagId = "1"});
                });
            
            using var svc = CreateCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.DeleteAsync($"{svc.ListenUri}/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).Should().Be(1);
            
            response = await cli.DeleteAsync($"{svc.ListenUri}/tag");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).Should().Be(0);
            var tags = await cli.GetAsync<List<Tag>>($"{svc.ListenUri}/tag");
            tags.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task Should_return_version()
        {
            using var svc = CreateCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.GetStringAsync($"{svc.ListenUri}/version");
            response.Should().Be("1.0.0.0");
        }
    }
}