using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using Newtonsoft.Json;
using Serilog;

namespace maxbl4.Race.Logic.CheckpointService.Client
{
    public class CheckpointServiceClient
    {
        private readonly ILogger logger = Log.ForContext<CheckpointServiceClient>();
        private readonly string address;
        
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
            var client = new HttpClient();
            return await client.GetAsync<List<Checkpoint>>($"{address}cp?start={start:u}&end={end:u}");
        }

        public async Task AppendCheckpoint(string riderId)
        {
            logger.Information("PutRiderId {riderId}", riderId);
            var cli = new HttpClient();
            (await cli.PutAsync($"{address}cp", 
                new StringContent(JsonConvert.SerializeObject(riderId), Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
        }

        public async Task<int> DeleteCheckpoint(Id<Checkpoint> id)
        {
            var cli = new HttpClient();
            var response = await cli.DeleteAsync($"{address}cp/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAs<int>();
        }
        
        public async Task<int> DeleteCheckpoints(DateTime? start = null, DateTime? end = null)
        {
            var cli = new HttpClient();
            var response = await cli.DeleteAsync($"{address}cp?start={start:u}&end={end:u}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAs<int>();
        }
    }
}