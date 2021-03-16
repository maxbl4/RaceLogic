using System.Threading.Tasks;
using maxbl4.Race.Logic.WsHub;
using maxbl4.Race.Logic.WsHub.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.CheckpointService.Controllers
{
    [ApiController]
    [Route("upstream-options")]
    public class UpstreamOptionsController : ControllerBase
    {
        private readonly IUpstreamOptionsStorage upstreamOptionsStorage;

        public UpstreamOptionsController(IUpstreamOptionsStorage upstreamOptionsStorage)
        {
            this.upstreamOptionsStorage = upstreamOptionsStorage;
        }

        [HttpGet]
        public Task<UpstreamOptions> Get()
        {
            return upstreamOptionsStorage.GetUpstreamOptions();
        }

        [HttpPost]
        [HttpPut]
        public async Task Put([FromBody] UpstreamOptions options)
        {
            await upstreamOptionsStorage.SetUpstreamOptions(options);
        }
    }
}