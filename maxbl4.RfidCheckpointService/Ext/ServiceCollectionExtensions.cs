using System;
using maxbl4.RfidCheckpointService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace maxbl4.RfidCheckpointService.Ext
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterHostedService<T>(this IServiceCollection serviceCollection) where T : class, IDisposable
        {
            serviceCollection.AddSingleton<T>();
            serviceCollection.AddSingleton<IHostedService, ServiceHost<T>>();
        }
    }
}