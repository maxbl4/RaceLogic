using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Race.CheckpointService.Model;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Tests.CheckpointService.RfidSimulator;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Tag = maxbl4.RfidDotNet.Tag;

namespace maxbl4.Race.Tests.CheckpointService.Controllers
{
    public class CheckpointsControllerTests : IntegrationTestBase
    {
        public CheckpointsControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task HttpClient_should_wait_for_webhost_to_start()
        {
            // Make service slow on startup by putting 5000ms sleep
            var sw = Stopwatch.StartNew();
            using var svc = CreateCheckpointService(5000);
            var client = new HttpClient();
            // Get data from service should succeed, but take 5000+ ms
            var response = await client.GetAsync($"{svc.ListenUri}/cp");
            (await response.Content.ReadAsStringAsync()).Should().Be("[]");
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeGreaterThan(5000);
        }

        [Fact]
        public async Task Should_return_stored_checkpoints()
        {
            var ts = DateTime.UtcNow;
            WithCheckpointStorageService(storageService =>
            {
                storageService.AppendCheckpoint(new Checkpoint("stored1", ts));
                storageService.AppendCheckpoint(new Checkpoint("stored2", ts.AddSeconds(100)));
            });
            
            using var svc = CreateCheckpointService();
            var client = new HttpClient();
            var checkpoints = await client.GetAsync<List<Checkpoint>>($"{svc.ListenUri}/cp");
            checkpoints.Should().NotBeNull();
            checkpoints.Count.Should().Be(2);
            checkpoints.Should().Contain(x => x.RiderId == "stored1");
            checkpoints.Should().Contain(x => x.RiderId == "stored2");
        }
        
        [Fact]
        public async Task Should_stream_checkpoints_over_websocket()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateCheckpointService();
            tagListHandler.ReturnOnce(new Tag{TagId = "1"});
            tagListHandler.ReturnOnce(new Tag{TagId = "2"});
            var wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{svc.ListenUri}/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint[] cp) => checkpoints.AddRange(cp));
            var wsConnected = false;
            wsConnection.On("ReaderStatus", (ReaderStatus s) => wsConnected = true);
            await new Timing().ExpectAsync(() => wsConnected);
            tagListHandler.ReturnOnce(new Tag{TagId = "3"});
            tagListHandler.ReturnOnce(new Tag{TagId = "4"});
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count >= 4);
        }
        
        [Fact]
        public async Task Should_append_manual_checkpoint()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateCheckpointService();
            var wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{svc.ListenUri}/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint[] cp) => checkpoints.AddRange(cp));
            var wsConnected = false;
            wsConnection.On("ReaderStatus", (ReaderStatus s) => wsConnected = true);
            await new Timing().ExpectAsync(() => wsConnected);
            var cli = new HttpClient();
            (await cli.PutAsync($"{svc.ListenUri}/cp", 
                new StringContent("\"555\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count >= 1);
            checkpoints.Should().Contain(x => x.RiderId == "555");
        }
        
        [Fact]
        public async Task Should_append_manual_checkpoint_with_rfid_disabled()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build(false));
            
            using var svc = CreateCheckpointService();
            var wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{svc.ListenUri}/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint[] cp) => checkpoints.AddRange(cp));
            var wsConnected = false;
            wsConnection.On("ReaderStatus", (ReaderStatus s) => wsConnected = true);
            var cli = new HttpClient();
            (await cli.PutAsync($"{svc.ListenUri}/cp", 
                new StringContent("\"555\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count == 1);
            checkpoints.Should().Contain(x => x.RiderId == "555");
            wsConnected.Should().BeFalse();
        }
        
        [Fact]
        public async Task Should_ignore_empty_manual_checkpoint()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateCheckpointService();
            var wsConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithUrl($"{svc.ListenUri}/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint[] cp) => checkpoints.AddRange(cp));
            var wsConnected = false;
            wsConnection.On("ReaderStatus", (ReaderStatus s) => wsConnected = true);
            await new Timing().ExpectAsync(() => wsConnected);
            var cli = new HttpClient();
            (await cli.PutAsync($"{svc.ListenUri}/cp", 
                new StringContent("\"\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await new Timing()
                .Timeout(5000)
                .Logger(Logger)
                .WaitAsync(() => checkpoints.Count > 0)).Should().BeFalse();
        }

        [Fact]
        public async Task Should_remove_checkpoints()
        {
            var now = DateTime.UtcNow;
            var original = new[]
            {
                new Checkpoint {RiderId = "111", Timestamp = now},
                new Checkpoint {RiderId = "222", Timestamp = now.AddMinutes(1)},
                new Checkpoint {RiderId = "333", Timestamp = now.AddMinutes(2)},
                new Checkpoint {RiderId = "444", Timestamp = now.AddMinutes(3)},
                new Checkpoint {RiderId = "555", Timestamp = now.AddMinutes(4)}
            };

            var tagListHandler = WithCheckpointStorageService(storageService =>
                {
                    foreach (var cp in original)
                    {
                        storageService.AppendCheckpoint(cp);    
                    }
                    return new SimulatorBuilder(storageService).Build();
                });
            
            using var svc = CreateCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.DeleteAsync($"{svc.ListenUri}/cp/{original[0].Id}");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).Should().Be(1);
            var cps = await cli.GetAsync<List<Checkpoint>>($"{svc.ListenUri}/cp");
            cps.Count.Should().Be(4);
            cps.Should().NotContain(x => x.Id == original[0].Id);
            
            response = await cli.DeleteAsync($"{svc.ListenUri}/cp?start={now.AddMinutes(1.5):u}&end={now.AddMinutes(3.5):u}");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).Should().Be(2);
            cps = await cli.GetAsync<List<Checkpoint>>($"{svc.ListenUri}/cp");
            cps.Count.Should().Be(2);
            cps.Should().NotContain(x => x.Id == original[2].Id || x.Id == original[3].Id);
            
            response = await cli.DeleteAsync($"{svc.ListenUri}/cp");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).Should().Be(2);
            cps = await cli.GetAsync<List<Checkpoint>>($"{svc.ListenUri}/cp");
            cps.Count.Should().Be(0);
        }
    }
}