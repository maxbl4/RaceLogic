using System;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using maxbl4.Race.DataService.Options;
using Microsoft.Extensions.Options;
using ServiceBase;

namespace maxbl4.Race.DataService.Services
{
    public class StorageService : StorageServiceBase
    {
        private readonly IOptions<ServiceOptions> serviceOptions;
     
        public StorageService(IOptions<ServiceOptions> serviceOptions,
            IMessageHub messageHub, ISystemClock systemClock) :
            base(serviceOptions.Value.StorageConnectionString, messageHub, systemClock)
        {
            this.serviceOptions = serviceOptions;
        }

        protected override void ValidateDatabase()
        {
        }

        protected override void SetupIndexes()
        {
        }
    }
}