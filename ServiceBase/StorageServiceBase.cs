using System;
using System.Reactive.PlatformServices;
using Easy.MessageHub;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.Extensions;
using Serilog;

namespace ServiceBase
{
    public abstract class StorageServiceBase : IDisposable
    {
        protected readonly string connectionString;
        protected readonly IMessageHub messageHub;
        protected readonly ISystemClock systemClock;
        protected readonly ILogger logger;
        protected LiteRepository repo;

        protected StorageServiceBase(string connectionString, IMessageHub messageHub, ISystemClock systemClock)
        {
            logger = Log.ForContext(GetType());
            this.connectionString = connectionString;
            this.messageHub = messageHub;
            this.systemClock = systemClock;
            var cs = new ConnectionString(connectionString);
            logger.SwallowError(() => Initialize(cs), ex =>
            {
                repo?.Dispose();
                cs = TryRotateDatabase(cs);
                Initialize(cs);
            });
        }

        private void Initialize(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).CurrentFile;
            logger.Information($"Using storage file {connectionString.Filename}");
            repo = LiteRepo.WithUtcDate(connectionString);
            SetupIndexes();
            ValidateDatabase();
        }

        private ConnectionString TryRotateDatabase(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).NextFile;
            return connectionString;
        }

        protected abstract void ValidateDatabase();

        protected abstract void SetupIndexes();
        
        public void Dispose()
        {
            repo.DisposeSafe();
        }
    }
}