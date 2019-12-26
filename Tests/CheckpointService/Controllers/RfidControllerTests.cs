using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Race.CheckpointService.Services;
using Newtonsoft.Json;
using Shouldly;
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
            opts.Enabled.ShouldBe(true);
            opts.ConnectionString.ShouldBe("some");
            opts.RpsThreshold.ShouldBe(123);
            opts.CheckpointAggregationWindowMs.ShouldBe(234);
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
            opts2.Enabled.ShouldBe(true);
            opts2.ConnectionString.ShouldBe("bbb");
            opts2.RpsThreshold.ShouldBe(666);
            opts2.CheckpointAggregationWindowMs.ShouldBe(777);
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
                .ShouldBe(true);
            (await client.GetAsync<string>($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}"))
                .ShouldBe("some");
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}"))
                .ShouldBe(123);
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.CheckpointAggregationWindowMs)}"))
                .ShouldBe(234);
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
            opts.Enabled.ShouldBeFalse();

            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}",
                new StringContent("555", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<int>($"{CheckpointsUri}/options/{nameof(RfidOptions.RpsThreshold)}"))
                .ShouldBe(555);
            
            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.Enabled)}",
                new StringContent("true", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<bool>($"{CheckpointsUri}/options/{nameof(RfidOptions.Enabled)}"))
                .ShouldBe(true);
            
            (await client.PutAsync($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}",
                new StringContent("\"true\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            (await client.GetAsync<string>($"{CheckpointsUri}/options/{nameof(RfidOptions.ConnectionString)}"))
                .ShouldBe("true");
        }
    }
}