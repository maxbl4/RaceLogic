using System;
using Microsoft.Extensions.Logging;
namespace maxbl4.RfidCheckpointService
{
    public static class Safe
    {
        public static void Execute(Action action, ILogger logger)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Operation failed");
            }
        }
    }
}