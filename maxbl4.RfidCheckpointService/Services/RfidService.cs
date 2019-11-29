using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoMapper;
using Easy.MessageHub;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RfidCheckpointService.Model;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Serilog;
using Tag = maxbl4.RfidCheckpointService.Model.Tag;

namespace maxbl4.RfidCheckpointService.Services
{
    public class RfidService : IRfidService
    {
        private readonly StorageService storageService;
        private readonly IMessageHub messageHub;
        private readonly ISystemClock systemClock;
        private readonly IMapper mapper;
        private readonly ILogger logger = Log.ForContext<RfidService>();
        private readonly UniversalTagStreamFactory factory;
        private IUniversalTagStream stream;
        private CompositeDisposable disposable;
        private TimestampCheckpointAggregator aggregator;
        private readonly Subject<Checkpoint> checkpoints = new Subject<Checkpoint>();

        public RfidService(StorageService storageService, IMessageHub messageHub,
            ISystemClock systemClock, IMapper mapper)
        {
            this.storageService = storageService;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            this.mapper = mapper;
            messageHub.Subscribe<RfidOptions>(RfidOptionsChanged);
            factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            factory.UseSerialProtocol();
            RfidOptionsChanged(storageService.GetRfidOptions());
        }

        private void RfidOptionsChanged(RfidOptions options)
        {
            DisableRfid();
            logger.Information("Using RfidOptions: {options}", options);
            if (ShouldStartRfid(options))
                logger.SwallowError(() => EnableRfid(options))
                    .Wait(0);
        }

        private bool ShouldStartRfid(RfidOptions options)
        {
            if (options.Enabled
                && systemClock.UtcNow.UtcDateTime - options.Timestamp > TimeSpan.FromDays(1))
            {
                options.Enabled = false;
                storageService.SetRfidOptions(options, false);
                return false;
            }

            return options.Enabled;
        }

        void OnCheckpoint(Checkpoint cp)
        {
            logger.Debug("OnCheckpoint {cp}", cp);
            logger.SwallowError(() => storageService.AppendCheckpoint(cp));
            logger.Swallow(() => messageHub.Publish(cp));
        }

        void OnReaderStatus(ReaderStatus status)
        {
            logger.Debug("OnReaderStatus {status}", status);
            logger.Swallow(() => messageHub.Publish(status));
        }
        
        private async Task EnableRfid(RfidOptions options)
        {
            logger.Information("Starting RFID");
            
            stream = factory.CreateStream(options.GetConnectionString());
            disposable = new CompositeDisposable(stream,
                stream.Tags.Subscribe(x =>
                {
                    if (options.PersistTags)
                        logger.Swallow(() => storageService.AppendTag(mapper.Map<Tag>(x)));
                    AppendRiderId(x.TagId);
                }));
            aggregator =
                new TimestampCheckpointAggregator(TimeSpan.FromMilliseconds(options.CheckpointAggregationWindowMs));
            disposable.Add(stream.Connected.CombineLatest(stream.Heartbeat,
                    (con, hb) => new ReaderStatus {IsConnected = con, Heartbeat = hb})
                .Subscribe(OnReaderStatus)
            );
            disposable.Add(checkpoints.Subscribe(aggregator));
            disposable.Add(aggregator.Subscribe(OnCheckpoint));
            disposable.Add(aggregator.AggregatedCheckpoints.Subscribe(OnCheckpoint));
            await stream.Start();
        }

        public void AppendRiderId(string riderId)
        {
            logger.Information($"Append riderId {riderId} at {systemClock.UtcNow.UtcDateTime:u}");
            checkpoints.OnNext(new Checkpoint(riderId, systemClock.UtcNow.UtcDateTime));
        }

        void DisableRfid()
        {
            disposable.DisposeSafe();
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
        }
    }
}