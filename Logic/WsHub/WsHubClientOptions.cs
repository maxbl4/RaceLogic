using System;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub
{
    public class WsHubClientOptions
    {
        public string Address { get; }
        public string AccessToken { get; }
        public ServiceFeatures Features { get; set; } = ServiceFeatures.None;
        public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan LastSeenMessageIdsRetentionPeriod { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan LastSeenMessageIdsCleanupPeriod { get; set; } = TimeSpan.FromSeconds(5);

        public WsHubClientOptions(string address, string accessToken)
        {
            if (!address.EndsWith("/"))
                address += "/";
            Address = address;
            AccessToken = accessToken;
        }
        
        public static WsHubClientOptions Empty => new WsHubClientOptions("", "");
    }
}