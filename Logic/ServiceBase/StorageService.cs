using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using maxbl4.Infrastructure;
using maxbl4.Infrastructure.Extensions.DisposableExt;
using maxbl4.Infrastructure.Extensions.LoggerExt;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.Extensions;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.ServiceBase
{
    public interface IRepository
    {
        IStorageService StorageService { get; }
    }

    public class StorageServiceOptions
    {
        public string StorageConnectionString { get; set; }
    }

    public interface IStorageService
    {
        ILiteRepository Repo { get; }
        IEnumerable<T> List<T, K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, K>> orderBy = null, int? skip = null, int? limit = null) where T : IHasId<T>;
        Id<T> Save<T>(T entity) where T : IHasId<T>;
        Id<T> Update<T>(Id<T> id, Action<T> modifier) where T : IHasId<T>;
        IEnumerable<T> List<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null) where T : IHasId<T>;
        T Get<T>(Id<T> id) where T : IHasId<T>;
        void RolloverDatabase();

        void Delete<T>(Id<T> id)
            where T : IHasId<T>;
    }
    
    public class StorageUpdated
    {
        public Guid Id { get; set; }
        public IHasGuidId Entity { get; set; }
    }
    
    public class StorageService : IStorageService, IDisposable
    {
        private readonly string connectionString;
        private readonly IMessageHub messageHub;
        private static readonly ILogger logger = Log.ForContext<StorageService>();
        private readonly Dictionary<Type, IRepository> consumers = new();
        public ILiteRepository Repo { get; private set; }
        public ConnectionString ConnectionString { get; private set; }

        public StorageService(IOptions<StorageServiceOptions> options, IMessageHub messageHub)
        {
            connectionString = options.Value.StorageConnectionString;
            this.messageHub = messageHub;
            ConnectionString = new ConnectionString(connectionString);
            logger.SwallowError(() => InitializeInt(ConnectionString), ex =>
            {
                Repo?.Dispose();
                ConnectionString = TryRotateDatabase(ConnectionString);
                InitializeInt(ConnectionString);
            });
        }

        public void Dispose()
        {
            Repo.DisposeSafe();
        }
        
        public T Get<T>(Id<T> id)
            where T : IHasId<T>
        {
            return Repo.FirstOrDefault<T>(x => x.Id == id);
        }
        
        public void Delete<T>(Id<T> id)
            where T : IHasId<T>
        {
            Repo.Delete<T>(id);
        }

        public void RolloverDatabase()
        {
            logger.Information("RolloverDatabase");
            ConnectionString = TryRotateDatabase(ConnectionString);
            logger.SwallowError(() => InitializeInt(ConnectionString), ex =>
            {
                Repo?.Dispose();
                ConnectionString = TryRotateDatabase(ConnectionString);
                InitializeInt(ConnectionString);
            });
        }

        public Id<T> Update<T>(Id<T> id, Action<T> modifier) where T : IHasId<T>
        {
            var dto = Get(id);
            modifier(dto);
            return Save(dto);
        }

        public IEnumerable<T> List<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null)
            where T : IHasId<T>
        {
            return List<T, object>(predicate, null, skip, limit);
        }

        public IEnumerable<T> List<T, K>(Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, K>> orderBy = null, int? skip = null, int? limit = null)
            where T : IHasId<T>
        {
            var query = Repo.Query<T>();
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = query.OrderBy(orderBy);
            if (skip != null || limit != null)
            {
                ILiteQueryableResult<T> result = null;
                if (skip != null) result = query.Skip(skip.Value);
                if (limit != null) result = query.Limit(limit.Value);
                return result.ToEnumerable();
            }

            return query.ToEnumerable();
        }

        public Id<T> Save<T>(T entity) where T : IHasId<T>
        {
            Repo.Upsert(entity.ApplyTraits());
            messageHub.Publish(new StorageUpdated{Id = entity.Id, Entity = entity});
            return entity.Id;
        }

        private void InitializeInt(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).CurrentFile;
            logger.Information($"Using storage file {connectionString.Filename}");
            Repo = LiteRepo.WithUtcDate(connectionString);
        }

        private ConnectionString TryRotateDatabase(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).NextFile;
            logger.Information("TryRotateDatabase {filename}", connectionString.Filename);
            return connectionString;
        }
    }
}