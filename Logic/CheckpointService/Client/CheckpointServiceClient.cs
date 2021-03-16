using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using Newtonsoft.Json;
using Serilog;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public class CheckpointServiceClient
    {
        private readonly string address;
        private readonly HttpClient http = new();
        private readonly ILogger logger = Log.ForContext<CheckpointServiceClient>();

        public CheckpointServiceClient(string address)
        {
            if (!address.EndsWith("/"))
                address += "/";
            this.address = address;
        }

        public ICheckpointSubscription CreateSubscription(DateTime from, TimeSpan? reconnectTimeout = null)
        {
            return new CheckpointSubscription(address, from, reconnectTimeout);
        }

        public async Task<List<Checkpoint>> GetCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            logger.Information("GetCheckpoints {start} {end}", start, end);
            return await http.GetAsync<List<Checkpoint>>($"{address}cp?start={start:u}&end={end:u}");
        }

        public async Task AppendCheckpoint(string riderId)
        {
            logger.Information("PutRiderId {riderId}", riderId);
            (await http.PutAsync($"{address}cp",
                    new StringContent(JsonConvert.SerializeObject(riderId), Encoding.UTF8, "application/json")))
                .EnsureSuccessStatusCode();
        }

        public async Task<int> DeleteCheckpoint(Id<Checkpoint> id)
        {
            var response = await http.DeleteAsync($"{address}cp/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAs<int>();
        }

        public async Task<int> DeleteCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var response = await http.DeleteAsync($"{address}cp?start={start:u}&end={end:u}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAs<int>();
        }

        public async Task<RfidOptions> GetRfidOptions()
        {
            return await http.GetAsync<RfidOptions>($"{address}options");
        }

        public async Task SetRfidOptions(RfidOptions options)
        {
            (await http.PutAsync($"{address}options",
                    new StringContent(JsonConvert.SerializeObject(options), Encoding.UTF8, "application/json")))
                .EnsureSuccessStatusCode();
        }

        public async Task<T> GetRfidOptionsValue<T>(string property)
        {
            return await http.GetAsync<T>($"{address}options/{property}");
        }

        public async Task SetRfidOptionsValue<T>(string property, T value)
        {
            (await http.PutAsync($"{address}options/{property}",
                    new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json")))
                .EnsureSuccessStatusCode();
        }

        public async Task<bool> GetRfidStatus()
        {
            return await GetRfidOptionsValue<bool>(nameof(RfidOptions.Enabled));
        }

        public async Task SetRfidStatus(bool enabled)
        {
            await SetRfidOptionsValue(nameof(RfidOptions.Enabled), enabled);
        }
    }
}