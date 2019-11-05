using maxbl4.RfidCheckpointService;
using Microsoft.Extensions.Hosting;

namespace maxbl4.RaceLogic.Tests.Ext
{
    public static class RfidCheckpointServiceRunnerExt
    {
        public static RfidCheckpointServiceRunner CreateDevelopment(string storageConnectionString,
            int pauseStartupMs = 0)
        {
            var svc = new RfidCheckpointServiceRunner();
            svc.Start(new []
            {
                $"--ServiceOptions:StorageConnectionString={storageConnectionString}", 
                $"--ServiceOptions:PauseInStartupMs={pauseStartupMs}",
                $"--Environment={Environments.Development}"
            }).Wait(0);
            return svc;
        }
    }
}