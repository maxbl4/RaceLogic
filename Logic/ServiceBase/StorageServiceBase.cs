﻿using System;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Race.Logic.Extensions;
using Serilog;

namespace maxbl4.Race.Logic.ServiceBase
{
    public abstract class StorageServiceBase : IDisposable
    {
        protected readonly string connectionString;
        protected readonly ILogger logger;
        protected LiteRepository repo;

        protected StorageServiceBase(string connectionString)
        {
            logger = Log.ForContext(GetType());
            this.connectionString = connectionString;
            var cs = new ConnectionString(connectionString);
            logger.SwallowError(() => Initialize(cs), ex =>
            {
                repo?.Dispose();
                cs = TryRotateDatabase(cs);
                Initialize(cs);
            });
        }

        public void Dispose()
        {
            repo.DisposeSafe();
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
    }
}