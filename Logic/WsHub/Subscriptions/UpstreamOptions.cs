using System;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class UpstreamOptions
    {
        public int Id => 1;
        public DateTime Timestamp { get; set; } = Constants.DefaultUtcDate;
        public WsHubClientOptions ConnectionOptions { get; set; } = WsHubClientOptions.Empty;
    }
}