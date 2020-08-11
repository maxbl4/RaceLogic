using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.CheckpointService.Model;
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
            var client = new CheckpointServiceClient(svc.ListenUri);
            // Get data from service should succeed, but take 5000+ ms
            var response = await client.GetCheckpoints();
            response.Should().BeEmpty();
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
            var client = new CheckpointServiceClient(svc.ListenUri);
            var checkpoints = await client.GetCheckpoints();
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
            var checkpoints = new List<Checkpoint>();
            var wsConnected = false;
            
            var client = new CheckpointServiceClient(svc.ListenUri);
            using var sub = client.CreateSubscription(DateTime.UtcNow.AddHours(-1));
            sub.Checkpoints.Subscribe(cp => checkpoints.Add(cp));
            sub.WebSocketConnected.Subscribe(s => wsConnected = s.IsConnected);
            sub.Start();
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
            
            var checkpoints = new List<Checkpoint>();
            var wsConnected = false;
            var client = new CheckpointServiceClient(svc.ListenUri);
            using var sub = client.CreateSubscription(DateTime.UtcNow.AddHours(-1));
            sub.Checkpoints.Subscribe(cp => checkpoints.Add(cp));
            sub.WebSocketConnected.Subscribe(s => wsConnected = s.IsConnected);
            sub.Start();
            await new Timing().ExpectAsync(() => wsConnected);
            await client.AppendCheckpoint("555");
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
            var checkpoints = new List<Checkpoint>();
            var wsConnected = false;
            ReaderStatus readerStatus = null;
            var client = new CheckpointServiceClient(svc.ListenUri);
            using var sub = client.CreateSubscription(DateTime.UtcNow.AddHours(-1));
            sub.Checkpoints.Subscribe(cp => checkpoints.Add(cp));
            sub.ReaderStatus.Subscribe(x => readerStatus = x);
            sub.WebSocketConnected.Subscribe(s => wsConnected = s.IsConnected);
            sub.Start();
            await new Timing().ExpectAsync(() => wsConnected);
            await client.AppendCheckpoint("555");
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count == 1);
            checkpoints.Should().Contain(x => x.RiderId == "555");
            wsConnected.Should().BeTrue();
            readerStatus.Should().NotBeNull();
            readerStatus.IsConnected.Should().Be(false);
        }
        
        [Fact]
        public async Task Should_ignore_empty_manual_checkpoint()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateCheckpointService();
            var checkpoints = new List<Checkpoint>();
            var wsConnected = false;
            var client = new CheckpointServiceClient(svc.ListenUri);
            using var sub = client.CreateSubscription(DateTime.UtcNow.AddHours(-1));
            sub.Checkpoints.Subscribe(cp => checkpoints.Add(cp));
            sub.WebSocketConnected.Subscribe(s => wsConnected = s.IsConnected);
            sub.Start();
            await new Timing().ExpectAsync(() => wsConnected);
            await client.AppendCheckpoint("");
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
            var client = new CheckpointServiceClient(svc.ListenUri);

            (await client.DeleteCheckpoint(original[0].Id)).Should().Be(1);
            var cps = await client.GetCheckpoints();
            cps.Count.Should().Be(4);
            cps.Should().NotContain(x => x.Id == original[0].Id);
            
            (await client.DeleteCheckpoints(now.AddMinutes(1.5), now.AddMinutes(3.5))).Should().Be(2);
            cps = await client.GetCheckpoints();
            cps.Count.Should().Be(2);
            cps.Should().NotContain(x => x.Id == original[2].Id || x.Id == original[3].Id);
            
            (await client.DeleteCheckpoints()).Should().Be(2);
            cps = await client.GetCheckpoints();
            cps.Count.Should().Be(0);
        }

        [Fact]
        public async Task Subscription_reconnects()
        {
            SystemClock.UseRealClock();
            var tagListHandler = WithCheckpointStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            var port = GetAvailablePort();
            var address = $"http://127.0.0.1:{port}";
            var client = new CheckpointServiceClient(address);
            var checkpoints = new List<Checkpoint>();
            var wsConnectionStatus = new List<WsConnectionStatus>();
            using var sub = (CheckpointSubscription)client.CreateSubscription(DateTime.UtcNow.AddHours(-1), TimeSpan.FromMilliseconds(500));
            sub.Checkpoints.Subscribe(cp => checkpoints.Add(cp));
            sub.WebSocketConnected.Subscribe(s => wsConnectionStatus.Add(s));
            sub.Start();

            // Assert we could not connect, but were trying
            await new Timing().ExpectAsync(() => wsConnectionStatus.Count > 2);
            wsConnectionStatus.Should().NotContain(x => x.IsConnected);
            
            using (CreateCheckpointService(port: port))
            {
                // Now we connect
                await new Timing().ExpectAsync(() => wsConnectionStatus.LastOrDefault()?.IsConnected == true);
                client.AppendCheckpoint("123");
                await new Timing().ExpectAsync(() => checkpoints.Any(cp => cp.RiderId == "123"));
            }
            // Disconnect again
            await new Timing().ExpectAsync(() => wsConnectionStatus.LastOrDefault()?.IsConnected == false);
            
            using (var svc = CreateCheckpointService(port: port))
            {
                // Connect again
                await new Timing().ExpectAsync(() => wsConnectionStatus.LastOrDefault()?.IsConnected == true);
                client.AppendCheckpoint("567");
                await new Timing().ExpectAsync(() => checkpoints.Any(cp => cp.RiderId == "567"));
            }
        }
        
        public static int GetAvailablePort()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            
            var tcpConnectionPorts = properties.GetActiveTcpConnections()
                .Select(n => n.LocalEndPoint.Port);
            var tcpListenerPorts = properties.GetActiveTcpListeners()
                .Select(n => n.Port);
            var udpListenerPorts = properties.GetActiveUdpListeners()
                .Select(n => n.Port);
            var port = Enumerable
                .Range(30000, 65535 - 30000)
                .Reverse()
                .Where(i => !tcpConnectionPorts.Contains(i))
                .Where(i => !tcpListenerPorts.Contains(i))
                .FirstOrDefault(i => !udpListenerPorts.Contains(i));

            return port;
        }
    }
}