using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using maxbl4.Infrastructure;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Messages;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using maxbl4.Race.Logic.WsHub.Subscriptions.Messages;
using maxbl4.Race.Tests.CheckpointService.RfidSimulator;
using maxbl4.Race.Tests.WsHub;
using maxbl4.RfidDotNet;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.CheckpointService
{
    public class SubscriptionManagerTests : IntegrationTestBase
    {
        public SubscriptionManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task Subscribe_and_stream_tags()
        {
            SystemClock.UseRealClock();
            using var wsHub = CreateWsHubService();
            var tagListHandler = WithCheckpointStorageService(storageService =>
            {
                storageService.SetUpstreamOptions(new UpstreamOptions
                {
                    ConnectionOptions = new WsHubClientOptions(wsHub.ListenUri, WsToken1)
                });
                return new SimulatorBuilder(storageService).Build();
            });

            using var svc = CreateCheckpointService();
            tagListHandler.ReturnOnce(new Tag {TagId = "1"});
            tagListHandler.ReturnOnce(new Tag {TagId = "2"});
            var checkpoints = new List<Checkpoint>();
            var rfidConnected = false;

            var client = new WsClientTestWrapper(new WsHubClientOptions(wsHub.ListenUri, WsToken2));
            client.Connection.RequestHandlers[typeof(ChekpointsUpdate)] = message =>
            {
                switch (message)
                {
                    case ChekpointsUpdate x:
                        if (x.Checkpoints != null)
                            checkpoints.AddRange(x.Checkpoints);
                        if (x.ReaderStatus != null)
                            rfidConnected = x.ReaderStatus.IsConnected;
                        return Task.FromResult<Message>(new ChekpointsUpdateResponse());
                }

                return null;
            };

            await client.Connect();
            await client.Connection.InvokeRequest<SubscriptionRequest, SubscriptionResponse>(WsToken1,
                new SubscriptionRequest
                {
                    FromTimestamp = DateTime.UtcNow.AddMinutes(-1),
                    Timeout = TimeSpan.FromSeconds(5)
                });

            await new Timing()
                .Logger(Logger)
                .ExpectAsync(() => rfidConnected);

            tagListHandler.ReturnOnce(new Tag {TagId = "3"});
            tagListHandler.ReturnOnce(new Tag {TagId = "4"});
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}, {string.Join(";", checkpoints)}")
                .ExpectAsync(() => checkpoints.Count >= 4);
        }
    }
}