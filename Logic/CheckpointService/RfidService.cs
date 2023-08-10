using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoMapper;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.CheckpointService.Model;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.AlienTech.Extensions;
using maxbl4.RfidDotNet.GenericSerial.Ext;
using Serilog;
using Tag = maxbl4.Race.Logic.CheckpointService.Model.Tag;

namespace maxbl4.Race.Logic.CheckpointService
{
    public class RfidService : IRfidService
    {
        private readonly Subject<Checkpoint> checkpoints = new();
        private readonly UniversalTagStreamFactory factory;
        private readonly ILogger logger = Log.ForContext<RfidService>();
        private readonly IMapper mapper;
        private readonly IMessageHub messageHub;
        private readonly CheckpointRepository checkpointRepository;
        private readonly ISystemClock systemClock;
        private TimestampAggregator<Checkpoint> aggregator;
        private CompositeDisposable aggregatorDisposable;
        private CompositeDisposable disposable;
        private IUniversalTagStream stream;

        public RfidService(CheckpointRepository checkpointRepository, IMessageHub messageHub,
            ISystemClock systemClock, IMapper mapper)
        {
            this.checkpointRepository = checkpointRepository;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            this.mapper = mapper;
            messageHub.Subscribe<RfidOptions>(RfidOptionsChanged);
            factory = new UniversalTagStreamFactory();
            factory.UseAlienProtocol();
            factory.UseSerialProtocol();
            RfidOptionsChanged(checkpointRepository.GetRfidOptions());
        }

        public void AppendRiderId(string riderId)
        {
            logger.Verbose($"Append riderId {riderId} at {systemClock.UtcNow.UtcDateTime:u}");
            checkpoints.OnNext(new Checkpoint(riderId, systemClock.UtcNow.UtcDateTime));
        }

        public void Dispose()
        {
            disposable.DisposeSafe();
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
                TimestampAggregatorConfigurations.ForCheckpoint(
                    TimeSpan.FromMilliseconds(options.CheckpointAggregationWindowMs));
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
                checkpointRepository.SetRfidOptions(options, false);
                return false;
            }

            return options.Enabled;
        }

        private void OnCheckpoint(Checkpoint cp)
        {
            logger.Debug("OnCheckpoint {cp}", cp);
            logger.SwallowError(() => checkpointRepository.AppendCheckpoint(cp));
            logger.Swallow(() => messageHub.Publish(cp));
        }

        private void OnReaderStatus(ReaderStatus status)
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
                stream.Errors.Subscribe(x => logger.Warning(x, "Rfid error")),
                stream.Tags.Subscribe(x =>
                {
                    if (options.PersistTags)
                        logger.Swallow(() => checkpointRepository.AppendTag(mapper.Map<Tag>(x)));
                    AppendRiderId(x.TagId);
                }),
                stream.Connected.CombineLatest(stream.Heartbeat,
                        (con, hb) => new ReaderStatus {IsConnected = con, Heartbeat = hb})
                    .Subscribe(OnReaderStatus)
            };
            await stream.Start();
        }

        private void DisableRfid()
        {
            disposable.DisposeSafe();
        }
    }
}