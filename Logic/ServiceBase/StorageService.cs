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
        void ValidateDatabase(ILiteRepository repo);
        void SetupIndexes(ILiteRepository repo);
    }

    public class StorageServiceOptions
    {
        public string StorageConnectionString { get; set; }
    }

    public interface IStorageService
    {
        ILiteRepository Repo { get; }
        List<T> List<T, K>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, K>> orderBy = null, int? skip = null, int? limit = null) where T : IHasId<T>;
        Id<T> Save<T>(T entity) where T : IHasId<T>;
        Id<T> Update<T>(Id<T> id, Action<T> modifier) where T : IHasId<T>;
        List<T> List<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null) where T : IHasId<T>;
        T Get<T>(Id<T> id) where T : IHasId<T>;
        void Initialize();
        T RegisterRepository<T>(Func<StorageService, T> consumer) where T: IRepository;
        T GetRepository<T>() where T: IRepository;
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

        public StorageService(IOptions<StorageServiceOptions> options, IMessageHub messageHub)
        {
            connectionString = options.Value.StorageConnectionString;
            this.messageHub = messageHub;
        }

        public void Initialize()
        {
            var cs = new ConnectionString(connectionString);
            logger.SwallowError(() => InitializeInt(cs), ex =>
            {
                Repo?.Dispose();
                cs = TryRotateDatabase(cs);
                InitializeInt(cs);
            });
        } 

        public T RegisterRepository<T>(Func<StorageService, T> consumer) where T: IRepository
        {
            var t = consumer(this);
            consumers[typeof(T)] = t;
            return t;
        }

        public T GetRepository<T>() where T: IRepository
        {
            return (T)consumers[typeof(T)];
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
        
        
        public Id<T> Update<T>(Id<T> id, Action<T> modifier) where T : IHasId<T>
        {
            var dto = Get(id);
            modifier(dto);
            return Save(dto);
        }

        public List<T> List<T>(Expression<Func<T, bool>> predicate = null, int? skip = null, int? limit = null)
            where T : IHasId<T>
        {
            return List<T, object>(predicate, null, skip, limit);
        }

        public List<T> List<T, K>(Expression<Func<T, bool>> predicate = null,
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
                return result.ToList();
            }

            return query.ToList();
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
            SetupIndexes();
            ValidateDatabase();
        }

        private ConnectionString TryRotateDatabase(ConnectionString connectionString)
        {
            connectionString.Filename = new RollingFileInfo(connectionString.Filename).NextFile;
            return connectionString;
        }

        private void ValidateDatabase()
        {
            foreach (var consumer in consumers.Values)
            {
                consumer.ValidateDatabase(Repo);
            }
        }

        private void SetupIndexes()
        {
            foreach (var consumer in consumers.Values)
            {
                consumer.SetupIndexes(Repo);
            }
        }
    }
}