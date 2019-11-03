using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidCheckpointService;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using Microsoft.AspNetCore.SignalR.Client;
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
        public async Task HttpClient_should_wait_for_webhost_to_start()
        {
            // add delay into webhost Startup and check, that tests still work
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Should_return_stored_checkpoints()
        {
            using var svc = new RfidCheckpointServiceRunner();
            svc.Start(new []{$"--StorageOptions:ConnectionString={storageConnectionString}"}).Wait(0);
            var ts = DateTime.UtcNow;
            storageService.AppendCheckpoint(new Checkpoint("1", ts));
            storageService.AppendCheckpoint(new Checkpoint("2", ts.AddSeconds(100)));
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:5001/cp");

            var checkpoints = JsonSerializer.Deserialize<List<Checkpoint>>(await response.Content.ReadAsStringAsync());
            checkpoints.ShouldNotBeNull();
            checkpoints.Count.ShouldBe(2);
        }
        
        [Fact]
        public async Task Should_stream_checkpoints_over_websocket()
        {
            var tagListHandler = new SimulatorBuilder(storageService).Build();
            using var svc = new RfidCheckpointServiceRunner();
            svc.Start(new []{$"--StorageOptions:ConnectionString={storageConnectionString}"}).Wait(0);
            var ts = DateTime.UtcNow;
            await tagListHandler.ReturnOnce(new Tag{TagId = "1"});
            await tagListHandler.ReturnOnce(new Tag{TagId = "2"});
            var wsConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint cp) => checkpoints.Add(cp));
            await tagListHandler.ReturnOnce(new Tag{TagId = "3"});
            await tagListHandler.ReturnOnce(new Tag{TagId = "4"});
            checkpoints.Count.ShouldBe(4);
        }
    }
}