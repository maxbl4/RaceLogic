using AutoMapper;
using maxbl4.RfidDotNet;

namespace maxbl4.Race.CheckpointService.Automapper
{
    public class MainProfile : Profile
    {
        public MainProfile()
        {
            CreateMap<Tag, Model.Tag>();
        }
    }
}