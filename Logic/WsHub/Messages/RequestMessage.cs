using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class RequestMessage : Message
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}