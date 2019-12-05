using AutoMapper;
using maxbl4.Race.CheckpointService.Model;

namespace maxbl4.Race.CheckpointService.Automapper
{
    public class MainProfile : Profile
    {
        public MainProfile()
        {
            CreateMap<RfidDotNet.Tag, Tag>();
        }
    }
}