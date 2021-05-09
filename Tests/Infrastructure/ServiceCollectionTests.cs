using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace maxbl4.Race.Tests.Infrastructure
{
    public class ServiceCollectionTests
    {
        [Fact]
        public void Returns_all_registered_interfaces()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService, Class1>();
            services.AddSingleton<IService, Class2>();
            services.AddSingleton<IService, Class3>();
            services.AddSingleton<Consumer>();
            var serviceProvider = services.BuildServiceProvider();
            var consumer = serviceProvider.GetService<Consumer>();
            consumer.Services.Should().HaveCount(3);
        }

        interface IService
        {
            
        }

        class Class1 : IService { }
        class Class2 : IService { }
        class Class3 : IService { }

        class Consumer
        {
            public List<IService> Services { get; }

            public Consumer(IEnumerable<IService> services)
            {
                Services = services.ToList();
            }
        }
    }
}