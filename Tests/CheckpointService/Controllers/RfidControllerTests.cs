using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.CheckpointService.Model;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.Race.Tests.CheckpointService.Controllers
{
    public class RfidControllerTests : IntegrationTestBase
    {
        public RfidControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task Should_return_rfid_options()
        {
            SystemClock.UseRealClock();
            WithCheckpointStorageService(storageService =>
                storageService.UpdateRfidOptions(opts =>
                {
                    opts.Enabled = true;
                    opts.ConnectionString = "some";
                    opts.RpsThreshold = 123;
                    opts.CheckpointAggregationWindowMs = 234;
                }));
            using var svc = CreateCheckpointService();
            var client = new CheckpointServiceClient(svc.ListenUri);
            var opts = await client.GetRfidOptions();
            opts.Enabled.Should().Be(true);
            opts.ConnectionString.Should().Be("some");
            opts.RpsThreshold.Should().Be(123);
            opts.CheckpointAggregationWindowMs.Should().Be(234);
        }

        [Fact]
        public async Task Should_set_rfid_options()
        {
            var opts = new RfidOptions
            {
                Enabled = true,
                ConnectionString = "bbb",
                RpsThreshold = 666,
                CheckpointAggregationWindowMs = 777
            };
            using var svc = CreateCheckpointService();
            var client = new CheckpointServiceClient(svc.ListenUri);
            await client.SetRfidOptions(opts);
            var opts2 = await client.GetRfidOptions();
            opts2.Enabled.Should().Be(true);
            opts2.ConnectionString.Should().Be("bbb");
            opts2.RpsThreshold.Should().Be(666);
            opts2.CheckpointAggregationWindowMs.Should().Be(777);
        }

        [Fact]
        public async Task Should_return_individual_option_property_values()
        {
            SystemClock.UseRealClock();
            WithCheckpointStorageService(storageService =>
                storageService.UpdateRfidOptions(opts =>
                {
                    opts.Enabled = true;
                    opts.ConnectionString = "some";
                    opts.RpsThreshold = 123;
                    opts.CheckpointAggregationWindowMs = 234;
                }));
            using var svc = CreateCheckpointService();
            var client = new CheckpointServiceClient(svc.ListenUri);
            (await client.GetRfidOptionsValue<bool>(nameof(RfidOptions.Enabled)))
                .Should().Be(true);
            (await client.GetRfidOptionsValue<string>(nameof(RfidOptions.ConnectionString)))
                .Should().Be("some");
            (await client.GetRfidOptionsValue<int>(nameof(RfidOptions.RpsThreshold)))
                .Should().Be(123);
            (await client.GetRfidOptionsValue<int>(nameof(RfidOptions.CheckpointAggregationWindowMs)))
                .Should().Be(234);
        }

        [Fact]
        public async Task Should_set_individual_option_property_values()
        {
            WithCheckpointStorageService(storageService =>
                storageService.UpdateRfidOptions(opts =>
                {
                    opts.Enabled = false;
                    opts.ConnectionString = "some";
                    opts.RpsThreshold = 111;
                    opts.CheckpointAggregationWindowMs = 222;
                }));
            using var svc = CreateCheckpointService();
            var client = new CheckpointServiceClient(svc.ListenUri);
            var opts = await client.GetRfidOptions();
            opts.Enabled.Should().BeFalse();

            await client.SetRfidOptionsValue(nameof(RfidOptions.RpsThreshold), 555);
            (await client.GetRfidOptionsValue<int>(nameof(RfidOptions.RpsThreshold)))
                .Should().Be(555);

            await client.SetRfidOptionsValue(nameof(RfidOptions.Enabled), true);
            (await client.GetRfidOptionsValue<bool>(nameof(RfidOptions.Enabled)))
                .Should().Be(true);

            await client.SetRfidOptionsValue(nameof(RfidOptions.ConnectionString), "true");
            (await client.GetRfidOptionsValue<string>(nameof(RfidOptions.ConnectionString)))
                .Should().Be("true");

            (await client.GetRfidStatus()).Should().BeTrue();
            await client.SetRfidStatus(false);
            (await client.GetRfidStatus()).Should().BeFalse();
        }
    }
}