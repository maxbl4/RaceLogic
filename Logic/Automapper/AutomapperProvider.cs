using AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventStorage.Storage.Model;

namespace maxbl4.Race.Logic.AutoMapper
{
    public interface IAutoMapperProvider
    {
        IMapper Mapper { get; }
    }

    public class AutoMapperProvider : IAutoMapperProvider
    {
        public IMapper Mapper { get; }

        public AutoMapperProvider()
        {
            var config = new MapperConfiguration(ConfigureMappings);
            Mapper = config.CreateMapper();
        }

        void ConfigureMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Checkpoint, CheckpointDto>();
        }
    }
}