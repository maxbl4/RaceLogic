using System.Reactive.PlatformServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace maxbl4.Race.WsHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, DefaultSystemClock>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
        }

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
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WsHub>("/ws/hub");
            });
        }
    }
}