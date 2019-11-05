using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using Easy.MessageHub;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Ext;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Microsoft.Extensions.Logging;

namespace maxbl4.RfidCheckpointService.Services
{
    public class RfidService : IDisposable
    {
        private readonly StorageService storageService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;
        private readonly ILogger<RfidService> logger;
        private readonly UniversalTagStreamFactory factory;
        private IUniversalTagStream stream;
        private CompositeDisposable disposable;
        private TimestampCheckpointAggregator aggregator;
        
        public RfidService(StorageService storageService, IMessageHub messageHub, 
            ISystemClock systemClock, ILogger<RfidService> logger)
        {
            this.storageService = storageService;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            this.logger = logger;
            messageHub.Subscribe<RfidOptions>(RfidOptionsChanged);
            factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            factory.UseSerialProtocol();
            RfidOptionsChanged(storageService.GetRfidOptions());
            
        }

        private void RfidOptionsChanged(RfidOptions options)
        {
            DisableRfid();
            logger.LogInformation("Using RfidOptions: {options}", options);
            aggregator = new TimestampCheckpointAggregator(TimeSpan.FromMilliseconds(options.CheckpointAggregationWindowMs));
            aggregator.Subscribe(OnCheckpoint);
            if (options.RfidEnabled)
                EnableRfid(options).WaitSafe(logger);
        }

        void OnCheckpoint(Checkpoint cp)
        {
            Safe.Execute(() => storageService.AppendCheckpoint(cp), logger);
            Safe.Execute(() => messageHub.Publish(cp), logger);
        }

        private async Task EnableRfid(RfidOptions options)
        {
            stream = factory.CreateStream(options.GetConnectionString());
            disposable = new CompositeDisposable(stream,
                stream.Tags.Select(x => new Checkpoint(x.TagId, systemClock.UtcNow.UtcDateTime)).Subscribe(aggregator));
            await stream.Start();
        }

        void DisableRfid()
        {
            disposable?.Dispose();
        }

        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}