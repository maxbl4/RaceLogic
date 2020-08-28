using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class PingRequest: Message, IRequestMessage
    {
        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(5);
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    public class PingResponse: Message
    {
        public DateTime SenderTimestamp { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}