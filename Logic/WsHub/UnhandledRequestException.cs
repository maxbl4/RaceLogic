using System;

namespace maxbl4.Race.Logic.WsHub
{
    public class UnhandledRequestException: Exception
    {
        public UnhandledRequestException(Exception innerException) : base("No handler defined for such request", innerException)
        {
        }
    }
}