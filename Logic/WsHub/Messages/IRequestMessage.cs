using System;

namespace maxbl4.Race.Logic.WsHub.Messages
{
    public interface IRequestMessage : IMessage
    {
        TimeSpan? Timeout { get; set; }
    }
}