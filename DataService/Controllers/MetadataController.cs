using maxbl4.Race.Logic.WebModel;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("_metadata")]
    public class MetadataController: ControllerBase
    {
        [HttpGet("get" + nameof(TimingSessionUpdate))]
        public TimingSessionUpdate GetTimingSessionUpdate()
        {
            return default;
        }
        
        [HttpGet("get" + nameof(ActiveTimingSessionsUpdate))]
        public ActiveTimingSessionsUpdate GetActiveTimingSessionsUpdate()
        {
            return default;
        }
    }
}