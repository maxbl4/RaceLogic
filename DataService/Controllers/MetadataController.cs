using maxbl4.Race.Logic.WebModel;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("_metadata")]
    public class MetadataController: ControllerBase
    {
        public TimingSessionUpdate GetTimingSessionUpdate()
        {
            return default;
        } 
    }
}