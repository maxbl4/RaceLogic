using System;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.Extensions;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.ServiceBase
{
    public interface ILiteRepoProvider : IDisposable
    {
        ConnectionString ConnectionString { get; }
        LiteRepository Repo { get; }
    }

    public class LiteRepoProvider : ILiteRepoProvider
    {
        private static readonly ILogger logger =  Log.ForContext<LiteRepoProvider>();
        public ConnectionString ConnectionString { get; private set; }
        private LiteRepository repo;

        public LiteRepoProvider(IOptions<LiteRepoProviderOptions> options)
        {
            ConnectionString = new ConnectionString(options.Value.ConnectionString);
            logger.SwallowError(() => Initialize(ConnectionString), ex =>
            {
                Repo?.Dispose();
                ConnectionString = TryRotateDatabase(ConnectionString);
                Initialize(ConnectionString);
            });
        }

        public LiteRepository Repo => repo;

        public void Dispose()
        {
            Repo.DisposeSafe();
        }

        private void Initialize(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).CurrentFile;
            logger.Information($"Using storage file {connectionString.Filename}");
            repo = LiteRepo.WithUtcDate(connectionString);
        }

        private ConnectionString TryRotateDatabase(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).NextFile;
            logger.Information($"Rotating lite db to {connectionString.Filename}");
            return connectionString;
        }
    }

    public class LiteRepoProviderOptions
    {
        public string ConnectionString { get; set; }
    }
}