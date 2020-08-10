using AutoMapper;
using maxbl4.Race.EventModel.Runtime;
using maxbl4.Race.EventModel.Storage.Model;
using maxbl4.Race.Logic.Checkpoints;

namespace maxbl4.Race.Logic.AutoMapper
{
    public interface IAutoMapperProvider
    {
        IMapper Mapper { get; }
        T Map<T>(object obj);
    }

    public class AutoMapperProvider : IAutoMapperProvider
    {
        public IMapper Mapper { get; }

        public AutoMapperProvider()
        {
            var config = new MapperConfiguration(ConfigureMappings);
            Mapper = config.CreateMapper();
        }

        public T Map<T>(object obj)
        {
            return Mapper.Map<T>(obj);
        }

        void ConfigureMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Checkpoint, CheckpointDto>();
            cfg.CreateMap<RecordingSessionDto, TimingSession>();
        }
    }
}