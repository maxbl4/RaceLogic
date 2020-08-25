using System.IO;
using System.Reactive.PlatformServices;
using System.Threading;
using AutoMapper;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Infrastructure.Extensions.ServiceCollectionExt;
using maxbl4.Race.CheckpointService.Hubs;
using maxbl4.Race.CheckpointService.Services;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using maxbl4.Race.Logic.WsHub.Subscriptions.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace maxbl4.Race.CheckpointService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonMapper.Global.RegisterIdBsonMappers();
            services.AddSingleton<ISystemClock, DefaultSystemClock>();
            services.AddSingleton<IMessageHub, MessageHub>();
            services.AddSingleton<StorageService>();
            services.AddSingleton<SubscriptionManager>();
            services.AddSingleton<ICheckpointStorage>(p => p.GetService<StorageService>());
            services.AddSingleton<IUpstreamOptionsStorage>(p => p.GetService<StorageService>());
            services.AddSingleton<ISubscriptionStorage>(p => p.GetService<StorageService>());
            services.AddAutoMapper(typeof(Startup));
            services.RegisterHostedService<IRfidService, RfidService>();
            services.RegisterHostedService<DistributionService>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR(options => options.MaximumReceiveMessageSize = 1 * 1024 * 1024).AddNewtonsoftJsonProtocol();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
            var options = Configuration.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();
            if (options?.PauseInStartupMs > 0)
                Thread.Sleep(options.PauseInStartupMs);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging(o =>
            {
                o.GetLevel = (context, duration, error) =>
                {
                    if (context.Request.Path.StartsWithSegments("/log"))
                        return LogEventLevel.Verbose;
                    return LogEventLevel.Information;
                };
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseDirectoryBrowser();
            app.UseStaticFiles();
            var shared = new SharedOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "var", "data")),
                RequestPath = "/files"
            };
            app.UseDirectoryBrowser(new DirectoryBrowserOptions(shared));
            app.UseStaticFiles(new StaticFileOptions(shared)
            {
                ServeUnknownFileTypes = true
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CheckpointsHub>("/_ws/cp");
            });
        }
    }
}