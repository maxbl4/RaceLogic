using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace maxbl4.Race.CheckpointService
{
    public class CheckpointServiceRunner : IDisposable
    {
        private CancellationTokenSource cts;
        private IHost hostBuilder;

        public async Task<int> Start(string[] args = null)
        {
            SetupLogger("appsettings");
            var logger = Log.ForContext<CheckpointServiceRunner>();
            Dispose();
            cts = new CancellationTokenSource();
            try
            {
                logger.Information("==== Host starting ====");
                hostBuilder = CreateHostBuilder(args ?? new string[0]).Build();
                await hostBuilder.RunAsync(cts.Token);
                return 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void SetupLogger(string configFileBaseName,string[] args = null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile($"{configFileBaseName}.json", true)
                .AddJsonFile($"{configFileBaseName}.Development.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseWebRoot(Path.Combine("var", "www"))
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
            hostBuilder?.StopAsync().Wait(5000);
        }
    }
}