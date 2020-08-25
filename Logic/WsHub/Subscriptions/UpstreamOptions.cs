using System;

namespace maxbl4.Race.Logic.WsHub.Subscriptions
{
    public class UpstreamOptions
    {
        public int Id => 1;
        public DateTime Timestamp { get; set; }
        public WsHubClientOptions ConnectionOptions { get; set; }
    }
}