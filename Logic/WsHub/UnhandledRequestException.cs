using System;

namespace maxbl4.Race.Logic.WsHub
{
    public class UnhandledRequestException : Exception
    {
        public UnhandledRequestException(Exception innerException) : base("No handler defined for such request",
            innerException)
        {
        }
    }

    public class DuplicateRequestException : Exception
    {
        public DuplicateRequestException(Guid messageId) : base(
            $"Request with id {messageId} is already executing", null)
        {
        }
    }
}