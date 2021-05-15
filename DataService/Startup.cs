using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.PlatformServices;
using BraaapWeb.Client;
using LiteDB;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.DataService.Options;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.AutoMapper;
using maxbl4.Race.Logic.CheckpointService.Client;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;
using maxbl4.Race.Logic.UpstreamData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            BsonMapper.Global.RegisterIdBsonMappers();
            services.AddSingleton<ISystemClock, DefaultSystemClock>();
            services.AddSingleton<IMessageHub, ChannelMessageHub>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<DataServiceRepository>();
            services.AddSingleton<IUpstreamDataRepository, UpstreamDataRepository>();
            services.AddSingleton<IEventRepository, EventRepository>();
            services.AddSingleton<IRecordingServiceRepository, RecordingServiceRepository>();
            services.AddSingleton<IRecordingService, RecordingService>();
            services.AddSingleton<ICheckpointServiceClientFactory, CheckpointServiceClientFactory>();
            services.AddSingleton<IAutoMapperProvider, AutoMapperProvider>();
            services.AddSingleton<ITimingSessionService, TimingSessionService>();
            services.AddSingleton<IUpstreamDataSyncService, UpstreamDataSyncService>();
            services.AddSingleton<ISeedDataLoader, SeedDataLoader>();
            services.AddSingleton<IHostedService, BootstrapService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers(o =>
            {
                o.ModelBinderProviders.Insert(0, new IdBinderProvider());
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                options.SerializerSettings.DateParseHandling = DateParseHandling.None;
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });
            });
            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
            var options = Configuration.GetSection(nameof(ServiceOptions)).Get<ServiceOptions>();
            services.Configure<StorageServiceOptions>(x => x.StorageConnectionString = options.StorageConnectionString);
            services.Configure<UpstreamDataSyncServiceOptions>(x =>
            {
                x.BaseUri = options.BraaapApiBaseUri;
                x.ApiKey = options.BraaapApiKey;
            });
            services.Configure<SeedDataLoaderOptions>(x =>
            {
                x.SeedDataDirectory = options.SeedDataDirectory;
            });
            services.AddTransient<IMainClient>(_ => new MainClient(options.BraaapApiBaseUri, new HttpClient()));
            services.AddOpenApiDocument(o =>
            {
                foreach (var mapper in TraitsExt.GetIHasIdTypes().Select(x => new PrimitiveTypeMapper(x, x =>
                    {
                        x.Type = JsonObjectType.String;
                        x.Format = "uuid";
                    })))
                {
                    o.TypeMappers.Add(mapper);                    
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();
            
            app.UseDefaultFiles();
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
            
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}