using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService;
using maxbl4.RfidCheckpointService.Services;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Controllers
{
    public class CheckpointsControllerTests : StorageServiceFixture
    {
        public CheckpointsControllerTests()
        {
            
        }

        [Fact]
        public async Task ShouldSupportAsyncEnumerable()
        {
            using var svc = new RfidCheckpointServiceRunner();
            svc.Start(new []{$"--StorageOptions:ConnectionString={storageConnectionString}"});
            var ts = DateTime.UtcNow;
            storageService.AppendCheckpoint(new Checkpoint("1", ts));
            storageService.AppendCheckpoint(new Checkpoint("2", ts.AddSeconds(100)));
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:5001/cp");

            var checkpoints = JsonSerializer.Deserialize<List<Checkpoint>>(await response.Content.ReadAsStringAsync());
            checkpoints.ShouldNotBeNull();
            checkpoints.Count.ShouldBe(2);
        }
    }
}