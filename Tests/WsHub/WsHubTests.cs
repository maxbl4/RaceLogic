using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.WsHub;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.WsHub
{
    public class WsHubTests: IntegrationTestBase
    {
        public WsHubTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task Send_simple_message()
        {
            using var svc = CreateWsHubService();
            using var cli1 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli1"});
            await cli1.Connect();
            using var cli2 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli2"});
            await cli2.Connect();

            await cli1.Client.SendToDirect("cli2", new TestMessage {Payload = "some"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli2.ClientMessages.OfType<TestMessage>().Any(x => x.Payload == "some"));
            cli1.ClientMessages.Should().BeEmpty();
            
            await cli2.Client.SendToDirect("cli1", new TestMessage {Payload = "222"});
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.OfType<TestMessage>().Any(x => x.Payload == "222"));
            cli2.ClientMessages.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task Messages_are_deduplicated()
        {
            using var svc = CreateWsHubService();
            using var cli1 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli1"});
            await cli1.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli1.Client.SendToDirect("cli1", msg);
            await cli1.Client.SendToDirect("cli1", msg);
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
            using var cli1 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli1"}, new WsHubClientOptions
            {
                LastSeenMessageIdsCleanupPeriod = TimeSpan.FromMilliseconds(300),
                LastSeenMessageIdsRetentionPeriod = TimeSpan.FromMilliseconds(200),
            });
            await cli1.Connect();
            var msg = new TestMessage{Payload = "Duplicate"};
            
            await cli1.Client.SendToDirect("cli1", msg);
            await Task.Delay(500);
            await cli1.Client.SendToDirect("cli1", msg);
            
            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => cli1.ClientMessages.OfType<TestMessage>().Count(x => x.Payload == "Duplicate") == 2);
        }
        
        [Fact]
        public async Task Send_to_multiple_clients_of_same_user()
        {
            using var svc = CreateWsHubService();
            using var sender = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "sender"});
            await sender.Connect();

            var tasks = Enumerable.Range(0, 5).Select(async x =>
            {
                var cli2 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "receiver"});
                await cli2.Connect();
                return cli2;
            }).ToList();
            await Task.WhenAll(tasks);
            
            await sender.Client.SendToDirect("receiver", new TestMessage {Payload = "some"});
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
            using (var cli = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration
            {
                ServiceId = "cli",
                Features = ServiceFeatures.RfidReader | ServiceFeatures.ManualInputTerminal
            }))
            {
                await cli.Connect();
                var regs = await cli.Client.ListServiceRegistrations();
                regs.Should().HaveCount(1);
                regs[0].ServiceId.Should().Be("cli");
                regs[0].Features.Should().Be(ServiceFeatures.RfidReader | ServiceFeatures.ManualInputTerminal);
            }

            using var cli2 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli2"});
            await cli2.Connect();
            await new Timing().ExpectAsync(async () => (await cli2.Client.ListServiceRegistrations()).All(x => x.ServiceId != "cli"));
        }
        
        [Fact]
        public async Task Send_to_topic()
        {
            using var svc = CreateWsHubService();
            using var cli = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli"});
            await cli.Connect();
            using var cli2 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli2"});
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
            msg.SenderId.Should().Be("cli2");
            
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
            using var cli = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli"});
            await cli.Connect();
            using var cli2 = new WsClientTestWrapper(svc.ListenUri, new ServiceRegistration{ServiceId = "cli2"});
            await cli2.Connect();
            cli.Client.RequestHandler = msg =>
            {
                var tst = (TestMessage) msg;
                tst.Payload += " hello";
                return Task.FromResult<Message>(tst);
            };
            var response = await cli2.Client.InvokeRequest<TestMessage>("cli", new TestMessage {Payload = "test"});
            response.Payload.Should().Be("test hello");
        }
    }

    public class WsClientTestWrapper: IDisposable
    {
        public ServiceRegistration Registration { get; }
        private readonly WsHubClient client;
        public List<Message> ClientMessages { get; } = new List<Message>();
        public List<WsConnectionStatus> ConnectionStatuses { get; } = new List<WsConnectionStatus>();

        public WsHubClient Client => client;

        public WsClientTestWrapper(string address, ServiceRegistration registration, WsHubClientOptions options = null)
        {
            Registration = registration;
            client = new WsHubClient(address, registration, options);
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
                .Logger(Log.ForContext(new PropertyEnricher(Constants.SourceContextPropertyName, $"{nameof(WsClientTestWrapper)}: {Registration}")))
                .ExpectAsync(() => ConnectionStatuses.LastOrDefault()?.IsConnected == true);
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}