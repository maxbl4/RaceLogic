using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace maxbl4.RfidCheckpointService.Services
{
    public class ServiceHost<T> : IHostedService
        where T: IDisposable
    {
        private T service;
        private readonly IServiceProvider serviceProvider;

        public ServiceHost(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            service = serviceProvider.GetService<T>();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            service?.Dispose();
            return Task.CompletedTask;
        }
    }
}