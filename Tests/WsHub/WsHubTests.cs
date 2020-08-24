using System;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace maxbl4.Race.Tests.WsHub
{
    public class WsHubTests
    {
        [Fact]
        public async Task Removes_outstanding_requests_on_timeout()
        {
            var hub = new Race.WsHub.WsHub();
            hub.Clients = Substitute.For<IHubCallerClients<IWsHubClient>>();
            hub.Context = Substitute.For<HubCallerContext>();
            await Assert.ThrowsAsync<HubException>(() => hub.InvokeRequest(JObject.FromObject(new RequestMessage
            {
                Timeout = TimeSpan.FromMilliseconds(10),
                Target = new MessageTarget {TargetId = "cli"}
            })));
            
            Race.WsHub.WsHub.OutstandingClientRequests.Should().BeEmpty();
        }
        
        [Fact]
        public async Task Removes_outstanding_requests_on_success()
        {
            var hub = new Race.WsHub.WsHub();
            hub.Clients = Substitute.For<IHubCallerClients<IWsHubClient>>();
            hub.Context = Substitute.For<HubCallerContext>();
            var messageId = new Id<Message>();
            var requestTask = hub.InvokeRequest(JObject.FromObject(new RequestMessage
            {
                MessageId = messageId,
                Timeout = TimeSpan.FromMilliseconds(1000),
                Target = new MessageTarget {TargetId = "cli"}
            }));
            hub.AcceptResponse(JObject.FromObject(new TestMessage
            {
                MessageId = messageId,
                Payload = "some"
            }));
            var response = Message.MaterializeConcreteMessage<TestMessage>(await requestTask);
            response.Payload.Should().Be("some");
            
            Race.WsHub.WsHub.OutstandingClientRequests.Should().BeEmpty();
        }
    }
}