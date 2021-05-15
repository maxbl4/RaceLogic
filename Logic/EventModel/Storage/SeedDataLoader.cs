using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LiteDB;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public interface ISeedDataLoader
    {
        void Load();
        void ResetAllData();
    }

    public class SeedDataLoader : ISeedDataLoader
    {
        private static readonly ILogger logger = Log.ForContext<SeedDataLoader>();
        private readonly SeedDataLoaderOptions options;
        private readonly IStorageService storageService;
        private readonly IMessageHub messageHub;

        public SeedDataLoader(IOptions<SeedDataLoaderOptions> options, IStorageService storageService, IMessageHub messageHub)
        {
            this.options = options.Value;
            this.storageService = storageService;
            this.messageHub = messageHub;
        }
        
        public void Load()
        {
            logger.Information("Load");
            var types = Assembly.GetAssembly(typeof(EventDto)).GetTypes().Where(x => x.IsAssignableTo(typeof(IHasTraits))).ToList();
            var files = Directory.GetFiles(options.SeedDataDirectory, "*.json");
            foreach (var file in files)
            {
                var type = types.FirstOrDefault(x =>
                    x.Name.Equals(Path.GetFileNameWithoutExtension(file), StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    logger.Information("Did not find type for {file}", Path.GetFileNameWithoutExtension(file));
                    continue;
                }
                
                logger.Information("Loading {file} into {type}", Path.GetFileNameWithoutExtension(file), type.Name);
                using var sr = new StreamReader(file);
                var data = JsonSerializer.Deserialize(sr);
                foreach (var obj in data.AsArray)
                {
                    var item = (IHasTraits)BsonMapper.Global.Deserialize(type, obj);
                    storageService.Repo.Upsert(item.ApplyTraits(), type.Name);
                }
                logger.Information("Inserted {count} documents", data.AsArray.Count);
            }
        }

        public void ResetAllData()
        {
            logger.Information("ResetAllData");
            messageHub.Publish(new ResetAllDataMessage());
            storageService.RolloverDatabase();
            Load();
        }
    }
    
    public class SeedDataLoaderOptions
    {
        public string SeedDataDirectory { get; set; }
    }

    public class ResetAllDataMessage
    {
        
    }
}