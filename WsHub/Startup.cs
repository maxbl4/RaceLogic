using System.Reactive.PlatformServices;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.WsHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using ISystemClock = System.Reactive.PlatformServices.ISystemClock;

namespace maxbl4.Race.WsHub
{
    public class Startup
    {
        private static readonly ILogger logger = Log.ForContext<Startup>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, DefaultSystemClock>();
            services.AddSingleton<IMessageHub, ChannelMessageHub>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<WsHubRepository>();
            services.AddAuthentication(Constants.WsHub.Authentication.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, WsAccessTokenAuthenticationHandler>(
                    Constants.WsHub.Authentication.SchemeName, options => { });

            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR(options => options.MaximumReceiveMessageSize = 1 * 1024 * 1024)
                .AddNewtonsoftJsonProtocol();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
            var options = Configuration.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();
            services.Configure<StorageServiceOptions>(x => x.StorageConnectionString = options.StorageConnectionString);
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
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WsHub>("/_ws/hub");
            });
        }
    }
}