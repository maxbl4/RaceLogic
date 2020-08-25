using System;
using maxbl4.Race.Logic.WsHub.Messages;

namespace maxbl4.Race.Logic.WsHub.Subscriptions.Messages
{
    public class SubscribeRequest: RequestMessage
    {
        public DateTime StartTimestamp { get; set; }
    }
    
    public class SubscribeResponse: Message {}
}