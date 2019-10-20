using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Services;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Microsoft.Extensions.Hosting;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using Microsoft.Extensions.Logging;

namespace maxbl4.RfidCheckpointService.Rfid
{
    public class RfidService : IHostedService
    {
        private readonly StorageService storageService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;
        private readonly ILogger<RfidService> logger;
        private RfidSettings settings;
        private readonly UniversalTagStreamFactory factory;
        private IUniversalTagStream stream;
        private CompositeDisposable disposable;
        private TimestampCheckpointAggregator aggregator;
        
        public RfidService(StorageService storageService, IMessageHub messageHub, 
            ISystemClock systemClock, ILogger<RfidService> logger, 
            Func<ConnectionString, IUniversalTagStream> fakeTagStreamFactory = null)
        {
            this.storageService = storageService;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            this.logger = logger;
            factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            factory.UseSerialProtocol();
            if (fakeTagStreamFactory != null)
                factory.UseFakeStream(fakeTagStreamFactory);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            settings = storageService.GetRfidSettings();
            aggregator = new TimestampCheckpointAggregator(settings.CheckpointAggregationWindow);
            aggregator.Subscribe(OnCheckpoint);
            if (settings.RfidEnabled)
                EnableRfid();
            return Task.CompletedTask;
        }

        void OnCheckpoint(Checkpoint cp)
        {
            Safe.Execute(() => messageHub.Publish(cp), logger);
            Safe.Execute(() => storageService.AppendCheckpoint(cp), logger);
        }

        public void EnableRfid()
        {
            stream = factory.CreateStream(settings.GetConnectionString());
            disposable = new CompositeDisposable(stream,
                stream.Tags.Select(x => new Checkpoint(x.TagId, systemClock.UtcNow.UtcDateTime)).Subscribe(aggregator));
        }

        public void DisableRfid()
        {
            disposable?.Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            disposable?.Dispose();
            return Task.CompletedTask;
        }
    }
}