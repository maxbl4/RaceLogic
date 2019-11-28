using AutoMapper;
using maxbl4.RfidCheckpointService.Model;

namespace maxbl4.RfidCheckpointService.Automapper
{
    public class MainProfile : Profile
    {
        public MainProfile()
        {
            CreateMap<RfidDotNet.Tag, Tag>();
        }
    }
}