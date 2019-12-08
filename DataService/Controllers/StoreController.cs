using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("store")]
    public class StoreController : ControllerBase
    {
        [HttpGet("{collection}/{id}")]
        public async Task<IActionResult> Get(string collection, long id)
        {
            return Ok();
        }
    }
}