using System.Reactive.PlatformServices;
using System.Threading;
using Easy.MessageHub;
using maxbl4.RfidCheckpointService.Ext;
using maxbl4.RfidCheckpointService.Hubs;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace maxbl4.RfidCheckpointService
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
            services.AddSingleton<ISystemClock, DefaultSystemClock>();
            services.AddSingleton<IMessageHub, MessageHub>();
            services.AddSingleton<StorageService>();
            services.RegisterHostedService<RfidService>();
            services.RegisterHostedService<DistributionService>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
            var options = Configuration.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();
            if (options?.PauseInStartupMs > 0)
                Thread.Sleep(options.PauseInStartupMs);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CheckpointsHub>("/ws/cp");
            });
        }
    }
}