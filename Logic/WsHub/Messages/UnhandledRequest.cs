using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public class UnhandledRequest : Message
    {
        public Exception Exception { get; set; }
    }
}