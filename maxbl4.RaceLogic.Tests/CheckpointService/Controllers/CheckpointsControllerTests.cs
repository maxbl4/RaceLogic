using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RaceLogic.Tests.Ext;
using maxbl4.RfidCheckpointService;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Controllers
{
    public class CheckpointsControllerTests : StorageServiceFixture
    {
        [Fact]
        public async Task HttpClient_should_wait_for_webhost_to_start()
        {
            storageService.Dispose();
            // Make service slow on startup by putting 5000ms sleep
            var sw = Stopwatch.StartNew();
            using var svc = RfidCheckpointServiceRunnerExt.CreateDevelopment(storageConnectionString, 5000);
            var client = new HttpClient();
            // Get data from service should succeed, but take 5000+ ms
            var response = await client.GetAsync("http://localhost:5000/cp");
            (await response.Content.ReadAsStringAsync()).ShouldBe("[]");
            sw.Stop();
            sw.ElapsedMilliseconds.ShouldBeGreaterThan(5000);
        }

        [Fact]
        public async Task Should_return_stored_checkpoints()
        {
            var ts = DateTime.UtcNow;
            storageService.AppendCheckpoint(new Checkpoint("stored1", ts));
            storageService.AppendCheckpoint(new Checkpoint("stored2", ts.AddSeconds(100)));
            storageService.Dispose();
            
            using var svc = RfidCheckpointServiceRunnerExt.CreateDevelopment(storageConnectionString);
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:5000/cp");

            var checkpoints = JsonSerializer.Deserialize<List<Checkpoint>>(await response.Content.ReadAsStringAsync());
            checkpoints.ShouldNotBeNull();
            checkpoints.Count(x => x.RiderId.StartsWith("stored")).ShouldBe(2);
        }
        
        [Fact]
        public async Task Should_stream_checkpoints_over_websocket()
        {
            var tagListHandler = new SimulatorBuilder(storageService).Build();
            storageService.Dispose();
            using var svc = RfidCheckpointServiceRunnerExt.CreateDevelopment(storageConnectionString);
            await tagListHandler.ReturnOnce(new Tag{TagId = "1"});
            await tagListHandler.ReturnOnce(new Tag{TagId = "2"});
            var wsConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint cp) => checkpoints.Add(cp));
            await tagListHandler.ReturnOnce(new Tag{TagId = "3"});
            await tagListHandler.ReturnOnce(new Tag{TagId = "4"});
            (await Timing.StartWait(() => checkpoints.Count >= 4)).ShouldBeTrue($"checkpoints.Count = {checkpoints.Count}");
        }
    }
}