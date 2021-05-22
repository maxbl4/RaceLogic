using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LiteDB;
using maxbl4.Infrastructure.MessageHub;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage.Traits;
using maxbl4.Race.Logic.ServiceBase;
using Microsoft.Extensions.Options;
using Serilog;

namespace maxbl4.Race.Logic.EventStorage.Storage
{
    public static class WellknownDtoIdentifiers
    {
        public static Id<OrganizationDto> OrganizationId => new Guid("1F95A21E-BBA9-42A3-9C7F-3EB16ACB5CA5");
        public static Id<GateDto> GateId => new Guid("C737C1F3-4EA2-4491-951F-7DBE339D7BCA");
    }
    
    public interface ISeedDataLoader
    {
        void Load(bool forceOverwrite);
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
        
        public void Load(bool forceOverwrite)
        {
            logger.Information("Load");
            var seed = storageService.List<SeedDataDto>().FirstOrDefault();
            if (seed != null && !forceOverwrite)
            {
                logger.Information("Load already loaded {at}", seed.Updated);
                return;
            }
            if (options.LoadHardcodedDefaults)
                LoadHardcodedDefaults();
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
            storageService.Save(seed ?? new SeedDataDto());
        }

        private void LoadHardcodedDefaults()
        {
            storageService.Save(new OrganizationDto { Id = WellknownDtoIdentifiers.OrganizationId, Name = "braaap.ru"});
            storageService.Save(new GateDto { Id = WellknownDtoIdentifiers.GateId, OrganizationId = WellknownDtoIdentifiers.OrganizationId, 
                Name = "Основные ворота", CheckpointServiceAddress = "http://localhost:6000", RfidSupported = true});
        }
    }
    
    public class SeedDataLoaderOptions
    {
        public bool LoadHardcodedDefaults { get; set; } = true;
        public string SeedDataDirectory { get; set; }
    }

    public class ResetAllDataMessage { }
}