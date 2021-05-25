using AutoMapper;
using maxbl4.Race.Logic.Checkpoints;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.RoundTiming;

namespace maxbl4.Race.Logic.AutoMapper
{
    public interface IAutoMapperProvider
    {
        IMapper Mapper { get; }
        T Map<T>(object obj);
    }

    public class AutoMapperProvider : IAutoMapperProvider
    {
        public AutoMapperProvider()
        {
            var config = new MapperConfiguration(ConfigureMappings);
            Mapper = config.CreateMapper();
        }

        public IMapper Mapper { get; }

        public T Map<T>(object obj)
        {
            return Mapper.Map<T>(obj);
        }

        private void ConfigureMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap(typeof(Id<>), typeof(Id<>));
            cfg.CreateMap<Checkpoint, CheckpointDto>().ReverseMap();
            cfg.CreateMap<RecordingSessionDto, TimingSession>().ReverseMap();
            cfg.CreateMap<RoundPosition, WebModel.RoundPosition>().ReverseMap();
        }
    }
}