using System;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Easy.MessageHub;
using maxbl4.Infrastructure;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Microsoft.Extensions.Logging;

namespace maxbl4.RfidCheckpointService.Services
{
    public class RfidService : IRfidService
    {
        private readonly StorageService storageService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;
        private readonly ILogger<RfidService> logger;
        private readonly UniversalTagStreamFactory factory;
        private IUniversalTagStream stream;
        private CompositeDisposable disposable;
        private TimestampCheckpointAggregator aggregator;
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();
        
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
            checkpoints.Subscribe(aggregator);
            aggregator.Subscribe(OnCheckpoint);
            if (options.Enabled)
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
                stream.Tags.Subscribe(x => AppendRiderId(x.TagId)));
            await stream.Start();
        }

        public void AppendRiderId(string riderId)
        {
            checkpoints.OnNext(new Checkpoint(riderId, systemClock.UtcNow.UtcDateTime));
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