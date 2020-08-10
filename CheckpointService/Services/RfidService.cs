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
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Serilog;
using Tag = maxbl4.Race.Logic.CheckpointService.Model.Tag;

namespace maxbl4.Race.CheckpointService.Services
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
        private CompositeDisposable aggregatorDisposable;
        private TimestampAggregator<Checkpoint> aggregator;
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
            ConfigureAggregator(options);
            if (ShouldStartRfid(options))
                logger.SwallowError(() => EnableRfid(options))
                    .Wait(0);
        }

        private void ConfigureAggregator(RfidOptions options)
        {
            aggregatorDisposable.DisposeSafe();
            aggregator =
                TimestampAggregatorConfigurations.ForCheckpoint(TimeSpan.FromMilliseconds(options.CheckpointAggregationWindowMs));
            aggregatorDisposable = new CompositeDisposable
            {
                checkpoints.Subscribe(aggregator),
                aggregator.Subscribe(OnCheckpoint),
                aggregator.AggregatedCheckpoints.Subscribe(OnCheckpoint)
            };
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
            disposable = new CompositeDisposable
            {
                stream,
                stream.Tags.Subscribe(x =>
                {
                    if (options.PersistTags)
                        logger.Swallow(() => storageService.AppendTag(mapper.Map<Tag>(x)));
                    AppendRiderId(x.TagId);
                }),
                stream.Connected.CombineLatest(stream.Heartbeat,
                        (con, hb) => new ReaderStatus {IsConnected = con, Heartbeat = hb})
                    .Subscribe(OnReaderStatus)
            };
            await stream.Start();
        }

        public void AppendRiderId(string riderId)
        {
            logger.Debug($"Append riderId {riderId} at {systemClock.UtcNow.UtcDateTime:u}");
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