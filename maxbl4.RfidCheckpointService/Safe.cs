using System;
using System.Threading.Tasks;
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
        
        public static async Task Execute(Func<Task> task, ILogger logger)
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Operation failed");
            }
        }
        
        public static async Task<T> Execute<T>(Func<Task<T>> task, ILogger logger)
        {
            try
            {
                return await task();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Operation failed");
                return default(T);
            }
        }

        public static void WaitSafe(this Task task, ILogger logger)
        {
            task.ContinueWith(x => logger.LogWarning(x.Exception, "Non-awaited task failed"), TaskContinuationOptions.OnlyOnFaulted)
                .Wait(0);
        }
    }
}