using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure;
using maxbl4.Race.CheckpointService.Hubs;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.Checkpoints;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.CheckpointService.Services
{
    public class DistributionServiceTests : IntegrationTestBase
    {
        public DistributionServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void Should_distribute()
        {
            // One client may throw in OnNext, but observable should not fail and continue sending
            WithCheckpointStorageService(storageService =>
            {
                var hubContext = Substitute.For<IHubContext<CheckpointsHub>>();
                var cps = new List<Checkpoint>();
                hubContext.Clients.Client("con1")
                    .SendCoreAsync("Checkpoint", Arg.Any<object[]>())
                    .Returns(Task.CompletedTask)
                    .AndDoes(info => cps.AddRange(info.ArgAt<object[]>(1).OfType<Checkpoint[]>().First()));

                var ds = new DistributionService(hubContext, MessageHub, storageService);
                ds.StartStream("con1", DateTime.UtcNow);


                MessageHub.Publish(new Checkpoint("r1", Constants.DefaultUtcDate));


                new Timing().Logger(Logger).Expect(() => cps.Count >= 1);
                cps[0].RiderId.Should().Be("r1");
            });
        }

        [Fact]
        public void Should_send_to_other_clients_when_one_fails()
        {
            // One client may throw in OnNext, but observable should not fail and continue sending
            WithCheckpointStorageService(storageService =>
            {
                var hubContext = Substitute.For<IHubContext<CheckpointsHub>>();
                var log = new List<string>();

                hubContext.Clients.Client("con1")
                    .SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>())
                    .ThrowsAsyncForAnyArgs(x => new ArgumentOutOfRangeException())
                    .AndDoes(info => log.Add("thrown"));

                hubContext.Clients.Client("con2")
                    .SendCoreAsync("Checkpoint", Arg.Any<object[]>())
                    .Returns(Task.CompletedTask)
                    .AndDoes(info => log.Add(info.ArgAt<object[]>(1)
                        .OfType<Checkpoint[]>().First()[0].RiderId));

                var ds = new DistributionService(hubContext, MessageHub, storageService);
                ds.StartStream("con1", DateTime.UtcNow);
                ds.StartStream("con2", DateTime.UtcNow);


                MessageHub.Publish(new Checkpoint("r1", Constants.DefaultUtcDate));


                new Timing().Logger(Logger).Expect(() => log.Count >= 2);
                log[0].Should().Be("thrown");
                log[1].Should().Be("r1");
            });
        }
    }
}