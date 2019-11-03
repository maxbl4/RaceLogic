using System.Reactive.PlatformServices;
using Easy.MessageHub;
using maxbl4.RfidCheckpointService.Hubs;
using maxbl4.RfidCheckpointService.Rfid;
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
            services.AddSingleton<RfidService>();
            services.AddSingleton<IHostedService, ServiceHost<RfidService>>();
            services.AddControllers();
            services.AddSignalR();
            services.Configure<StorageOptions>(Configuration.GetSection(nameof(StorageOptions)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CheckpointsHub>("/ws/cp");
            });
        }
    }
}