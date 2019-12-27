using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Race.CheckpointService.Services;
using Newtonsoft.Json;
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
            var client = new HttpClient();
            var opts = await client.GetAsync<RfidOptions>($"{CheckpointsUri}/options");
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
            var client = new HttpClient();
            (await client.PutAsync($"{CheckpointsUri}/options",
                new StringContent(JsonConvert.SerializeObject(opts), Encoding.UTF8, "application/json")))
                .EnsureSuccessStatusCode();
            var opts2 = await client.GetAsync<RfidOptions>($"{CheckpointsUri}/options");
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
            
            var client = new HttpClient();
            (await client.GetAsync<bool>($"{CheckpointsUri}/options/{nameof(RfidOptions.Enabled)}"))
                .Should().Be(true);
            (await client.GetAsync<string>($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}"))
                .Should().Be("some");
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}"))
                .Should().Be(123);
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.CheckpointAggregationWindowMs)}"))
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
            var client = new HttpClient();
            var opts = await client.GetAsync<RfidOptions>($"{CheckpointsUri}/options");
            opts.Enabled.Should().BeFalse();

            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}",
                new StringContent("555", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}"))
                .Should().Be(555);
            
            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.Enabled)}",
                new StringContent("true", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<bool>($"{CheckpointsUri}/options/{nameof(RfidOptions.Enabled)}"))
                .Should().Be(true);
            
            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}",
                new StringContent("\"true\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<string>($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}"))
                .Should().Be("true");
        }
    }
}