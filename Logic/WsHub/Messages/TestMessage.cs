using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class TestMessage : Message
    {
        public string Payload { get; set; }
    }

    public class TestRequest : Message, IRequestMessage
    {
        public string Payload { get; set; }
        public TimeSpan? Timeout { get; set; }
    }
}