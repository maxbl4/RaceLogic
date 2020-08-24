using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.Race.Logic.WsHub.Model;
using Newtonsoft.Json;
using Serilog;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubManagementClient
    {
        private readonly string address;
        private readonly ILogger logger = Log.ForContext<WsHubManagementClient>();
        private readonly HttpClient http;
        
        public WsHubManagementClient(string address, string adminAccessToken)
        {
            if (!address.EndsWith("/"))
                address += "/";
            this.address = address + "tokens/";
            this.http = new HttpClient
            {
                DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue(Constants.WsHub.Authentication.SchemeName, adminAccessToken)}
            };
        }
        
        public async Task<List<AuthToken>> GetTokens()
        {
            logger.Information("GetTokens");
            return await http.GetAsync<List<AuthToken>>(address);
        }
        
        public async Task UpsertToken(AuthToken token)
        {
            logger.Information("UpsertToken");
            (await http.PostAsync(address, new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json")))
                .EnsureSuccessStatusCode();
        }
        
        public async Task DeleteToken(string tokenValue)
        {
            logger.Information("UpsertToken");
            (await http.DeleteAsync($"{address}{WebUtility.UrlEncode(tokenValue)}"))
                .EnsureSuccessStatusCode();
        }
        
    }
}