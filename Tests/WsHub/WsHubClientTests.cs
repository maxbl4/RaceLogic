using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
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
            using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli1.Connect();
            using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();

            await cli1.Client.SendToDirect(WsToken2, new TestMessage {Payload = "some"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli2.ClientMessages.ToList().OfType<TestMessage>().Any(x => x.Payload == "some"));
            cli1.ClientMessages.Should().BeEmpty();
            
            await cli2.Client.SendToDirect(WsToken1, new TestMessage {Payload = "222"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.ToList().OfType<TestMessage>().Any(x => x.Payload == "222"));
            cli2.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Messages_are_deduplicated()
        {
            using var svc = CreateWsHubService();
            using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli1.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli1.Client.SendToDirect(WsToken1, msg);
            await cli1.Client.SendToDirect(WsToken1, msg);
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.OfType<TestMessage>().Any(x => x.Payload == "Duplicate"));
            await Task.Delay(1000);
            cli1.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Message_deduplication_cache_is_cleared()
        {
            using var svc = CreateWsHubService();
            using var cli1 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1)
            {
                LastSeenMessageIdsCleanupPeriod = TimeSpan.FromMilliseconds(300),
                LastSeenMessageIdsRetentionPeriod = TimeSpan.FromMilliseconds(200),
            });
            await cli1.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli1.Client.SendToDirect(WsToken1, msg);
            await Task.Delay(500);
            await cli1.Client.SendToDirect(WsToken1, msg);
            
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.OfType<TestMessage>().Count(x => x.Payload == "Duplicate") == 2);
        }
        
        [Fact]
        public async Task Send_to_multiple_clients_of_same_user()
        {
            using var svc = CreateWsHubService();
            using var sender = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await sender.Connect();

            var tasks = Enumerable.Range(0, 5).Select(async x =>
            {
                var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
                await cli2.Connect();
                return cli2;
            }).ToList();
            await Task.WhenAll(tasks);
            
            await sender.Client.SendToDirect(WsToken2, new TestMessage {Payload = "some"});
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
            using (var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1)
            {
                Features = ServiceFeatures.RfidReader | ServiceFeatures.ManualInputTerminal
            }))
            {
                await cli.Connect();
                var regs = await cli.Client.ListServiceRegistrations();
                regs.Should().HaveCount(1);
                regs[0].ServiceId.Should().Be(WsToken1);
                regs[0].Features.Should().Be(ServiceFeatures.RfidReader | ServiceFeatures.ManualInputTerminal);
            }

            using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            await new Timing().ExpectAsync(async () => (await cli2.Client.ListServiceRegistrations()).All(x => x.ServiceId != WsToken1));
        }
        
        [Fact]
        public async Task Send_to_topic()
        {
            using var svc = CreateWsHubService();
            using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            await cli.Client.Subscribe("topic1");
            await cli2.Client.Subscribe("topic1");
            await cli2.Client.SendToTopic("topic1", new TestMessage {Payload = "test topic"});
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
            await cli.Client.Unsubscribe("topic1");
            await cli2.Client.SendToTopic("topic1", new TestMessage {Payload = "test2"});
            await Task.Delay(100);
            cli.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Invoke_request()
        {
            using var svc = CreateWsHubService();
            using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();
            cli.Client.RequestHandler = msg =>
            {
                var tst = (TestMessage) msg;
                return Task.FromResult<Message>(new TestMessage
                {
                    Payload = tst.Payload + " hello"
                });
            };
            var response = await cli2.Client.InvokeRequest<TestMessage>(WsToken1, new TestMessage {Payload = "test"});
            response.Payload.Should().Be("test hello");
        }
        
        [Fact]
        public async Task Invoke_request_returns_unhandled()
        {
            using var svc = CreateWsHubService();
            using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken1));
            await cli.Connect();
            using var cli2 = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), WsToken2));
            await cli2.Connect();

            await Assert.ThrowsAsync<UnhandledRequestException>(() =>
                cli.Client.InvokeRequest<UnhandledRequest>(WsToken2, new TestMessage()));
        }
        
        [Fact]
        public async Task Authentication_failure()
        {
            using var svc = CreateWsHubService();
            using var cli = new WsClientTestWrapper(new WsHubClientOptions(GetHubAddress(svc), "Bad token"));
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

    public class WsClientTestWrapper: IDisposable
    {
        public WsHubClientOptions Options { get; }
        private readonly WsHubClient client;
        public List<Message> ClientMessages { get; } = new List<Message>();
        public List<WsConnectionStatus> ConnectionStatuses { get; } = new List<WsConnectionStatus>();

        public WsHubClient Client => client;

        public WsClientTestWrapper(WsHubClientOptions options)
        {
            Options = options;
            client = new WsHubClient(options);
            Client.WebSocketConnected.Subscribe(ConnectionStatuses.Add);
            Client.Messages.Subscribe(ClientMessages.Add);
        }
        
        public async Task Connect(bool expectSuccess = true)
        {
            await Client.Connect();
            if (expectSuccess)
                await ExpectConnected();
        }
        
        public async Task ExpectConnected()
        {
            await new Timing()
                .Logger(Log.ForContext(new PropertyEnricher(Constants.SourceContextPropertyName, $"{nameof(WsClientTestWrapper)}: {Options.Features}")))
                .ExpectAsync(() => ConnectionStatuses.LastOrDefault()?.IsConnected == true);
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}