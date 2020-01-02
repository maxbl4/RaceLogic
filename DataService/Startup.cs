using System.Reactive.PlatformServices;
using AutoMapper;
using Easy.MessageHub;
using maxbl4.Infrastructure.Extensions.ServiceCollectionExt;
using maxbl4.Race.DataService.Options;
using maxbl4.Race.DataService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace maxbl4.Race.DataService
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
            services.AddAutoMapper(typeof(Startup));
            services.RegisterHostedService<CheckpointServiceClient>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}