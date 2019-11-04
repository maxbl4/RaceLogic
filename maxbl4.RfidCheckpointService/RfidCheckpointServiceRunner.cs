using System;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.Ext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace maxbl4.RfidCheckpointService
{
    public class RfidCheckpointServiceRunner : IDisposable
    {
        private CancellationTokenSource cts;
        public async Task<int> Start(string[] args = null)
        {
            Dispose();
            cts = new CancellationTokenSource();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("RfidCheckpointServiceRunner.log")
                .CreateLogger();
            try
            {
                await CreateHostBuilder(args ?? new string[0]).Build().RunAsync(cts.Token);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseSerilog((builder, loggerConfig) =>
                            {
                                loggerConfig.ReadFrom.Configuration(builder.Configuration);
                            });
                });

        public void Dispose()
        {
            cts?.Cancel();
            cts.DisposeSafe();
        }
    }
}