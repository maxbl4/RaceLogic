using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
using ServiceBase;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.WsHub
{
    public class WsHubClientTests: IntegrationTestBase
    {
        public WsHubClientTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task Send_simple_message()
        {
            using var svc = CreateWsHubService();
            await using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli1.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();

            await cli1.Connection.SendToDirect(WsToken2, new TestMessage {Payload = "some"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli2.ClientMessages.ToList().OfType<TestMessage>().Any(x => x.Payload == "some"));
            cli1.ClientMessages.Should().BeEmpty();
            
            await cli2.Connection.SendToDirect(WsToken1, new TestMessage {Payload = "222"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.ToList().OfType<TestMessage>().Any(x => x.Payload == "222"));
            cli2.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Messages_are_deduplicated()
        {
            using var svc = CreateWsHubService();
            await using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli1.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli1.Connection.SendToDirect(WsToken2, msg);
            await cli1.Connection.SendToDirect(WsToken2, msg);
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli2.ClientMessages.OfType<TestMessage>().Any(x => x.Payload == "Duplicate"));
            await Task.Delay(1000);
            cli2.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Message_deduplication_cache_is_cleared()
        {
            using var svc = CreateWsHubService();
            await using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1)
            {
                LastSeenMessageIdsCleanupPeriod = TimeSpan.FromMilliseconds(300),
                LastSeenMessageIdsRetentionPeriod = TimeSpan.FromMilliseconds(200),
            });
            await cli1.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli2.Connection.SendToDirect(WsToken1, msg);
            await Task.Delay(500);
            await cli2.Connection.SendToDirect(WsToken1, msg);
            
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.OfType<TestMessage>().Count(x => x.Payload == "Duplicate") == 2);
        }
        
        [Fact]
        public async Task Send_to_multiple_clients_of_same_user()
        {
            using var svc = CreateWsHubService();
            await using var sender = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await sender.Connect();

            var tasks = Enumerable.Range(0, 5).Select(async x =>
            {
                var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
                await cli2.Connect();
                return cli2;
            }).ToList();
            await Task.WhenAll(tasks);
            
            await sender.Connection.SendToDirect(WsToken2, new TestMessage {Payload = "some"});
            for (var i = 0; i < 5; i++)
            {
                await new Timing()
                    .Logger(Logger)
                    .ExpectAsync(() => tasks[i].Result.ClientMessages.OfType<TestMessage>().Any(x => x.Payload == "some"));
            }
        }
        
        [Fact]
        public async Task Register_service_and_list_and_unregister()
        {
            using var svc = CreateWsHubService();
            await using (var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1)
            {
                Features = ServiceFeatures.CheckpointService | ServiceFeatures.ManualInputTerminal
            }))
            {
                await cli.Connect();
                var regs = await cli.Connection.ListServiceRegistrations();
                regs.Should().HaveCount(1);
                regs[0].ServiceId.Should().Be(WsToken1);
                regs[0].Features.Should().Be(ServiceFeatures.CheckpointService | ServiceFeatures.ManualInputTerminal);
            }

            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            await new Timing().ExpectAsync(async () => (await cli2.Connection.ListServiceRegistrations()).All(x => x.ServiceId != WsToken1));
        }
        
        [Fact]
        public async Task Send_to_topic()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            await cli.Connection.Subscribe("topic1");
            await cli2.Connection.Subscribe("topic1");
            await cli2.Connection.SendToTopic("topic1", new TestMessage {Payload = "test topic"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli.ClientMessages.Count > 0);
            cli.ClientMessages.Should().HaveCount(1);
            var msg = cli.ClientMessages.OfType<TestMessage>().First();
            msg.Payload.Should().Be("test topic");
            msg.Target.Type.Should().Be(TargetType.Topic);
            msg.Target.TargetId.Should().Be("topic1");
            msg.SenderId.Should().Be(WsToken2);
            
            cli2.ClientMessages.Should().BeEmpty();
            await cli.Connection.Unsubscribe("topic1");
            await cli2.Connection.SendToTopic("topic1", new TestMessage {Payload = "test2"});
            await Task.Delay(100);
            cli.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Invoke_request()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            var counter = 0;
            cli.Connection.RegisterRequestHandler((TestRequest msg) => Task.FromResult<Message>(new TestMessage
            {
                Payload = msg.Payload + " " + counter++
            }));
            var responses = new List<TestMessage>();
            for (var i = 0; i < 5; i++)
            {
                responses.Add(await cli2.Connection.InvokeRequest<TestRequest, TestMessage>(WsToken1, new TestRequest {Payload = "test", Timeout = TimeSpan.FromSeconds(5)}));
            }

            responses = responses.OrderBy(x => x.Payload).ToList();
            responses.Should().HaveCount(5);
            responses[0].Payload.Should().Be("test 0");
            responses[1].Payload.Should().Be("test 1");
            responses[2].Payload.Should().Be("test 2");
            responses[3].Payload.Should().Be("test 3");
            responses[4].Payload.Should().Be("test 4");
            cli.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
            cli2.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
        }
        
        [Fact]
        public async Task Invoke_request_returns_unhandled()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();

            await Assert.ThrowsAsync<UnhandledRequestException>(() =>
                cli.Connection.InvokeRequest<TestRequest, TestMessage>(WsToken2, new TestRequest{Timeout = TimeSpan.FromSeconds(1)}));
            cli.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
            cli2.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
        }
        
        [Fact]
        public async Task Invoke_request_timeouts()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            cli.Connection.RegisterRequestHandler(async (TestRequest msg) =>
            {
                await Task.Delay(3000);
                return null;
            });
            await Assert.ThrowsAsync<TimeoutException>(() => cli2.Connection.InvokeRequest<TestRequest, TestMessage>(WsToken1,
                new TestRequest {Payload = "test", Timeout = TimeSpan.FromSeconds(1)}));
            cli.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
            cli2.Connection.GetOutstandingRequestIds().Should().HaveCount(0);
        }
        
        [Fact]
        public async Task Ping()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            await using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            var ping = new PingRequest();
            var pong = await ((IPingRequester)cli2.Connection).Invoke(WsToken1, ping);
            pong.SenderTimestamp.Should().Be(ping.Timestamp);
            pong.Timestamp.Should().BeAfter(ping.Timestamp);

            await Assert.ThrowsAsync<HubException>(() => ((IPingRequester) cli.Connection).Invoke(WsToken1, ping));
        }

        [Fact]
        public async Task Authentication_failure()
        {
            using var svc = CreateWsHubService();
            await using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), "Bad token"));
            _ = cli.Connect();
            await new Timing().Logger(Logger)
                .ExpectAsync(() => cli.ConnectionStatuses.Any(x => x.Exception != null));
            var status = cli.ConnectionStatuses.First(x => x.Exception != null);
            status.Exception.Should().BeOfType<HttpRequestException>()
                .Which.Message.Should().Contain("401");
        }
        
        private string GetHubAddress(ServiceRunner<maxbl4.Race.WsHub.Startup> svc)
        {
            return svc.ListenUri;
        }
    }

    public class WsClientTestWrapper: IAsyncDisposable
    {
        public WsHubClientOptions Options { get; }
        private readonly WsHubConnection connection;
        public List<Message> ClientMessages { get; } = new();
        public List<WsConnectionStatus> ConnectionStatuses { get; } = new();

        public WsHubConnection Connection => connection;

        public WsClientTestWrapper(WsHubClientOptions options)
        {
            Options = options;
            connection = new WsHubConnection(options);
            Connection.WebSocketConnected.Subscribe(ConnectionStatuses.Add);
            Connection.MessageHandler = msg =>
            {
                ClientMessages.Add(msg);
                return Task.CompletedTask;
            };
        }
        
        public async Task Connect(bool expectSuccess = true)
        {
            await Connection.Connect();
            if (expectSuccess)
                await ExpectConnected();
        }
        
        public async Task ExpectConnected()
        {
            await new Timing()
                .Logger(Log.ForContext(new PropertyEnricher(Constants.SourceContextPropertyName, $"{nameof(WsClientTestWrapper)}: {Options.Features}")))
                .ExpectAsync(() => ConnectionStatuses.LastOrDefault()?.IsConnected == true);
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsyncSafe();
        }
    }
}