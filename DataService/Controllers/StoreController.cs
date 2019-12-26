using System.IO;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using maxbl4.Race.DataService.Services;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("store")]
    public class StoreController : ControllerBase
    {
        private readonly StorageService storageService;

        public StoreController(StorageService storageService)
        {
            this.storageService = storageService;
        }
        
        [HttpGet("{collection}/search")]
        public IActionResult Search(string collection, string query, int limit)
        {
            return Ok(storageService.Search(collection, query, limit));
        }
        
        [HttpGet("{collection}/count")]
        public IActionResult Count(string collection, string query)
        {
            return Ok(storageService.Count(collection, query));
        }
        
        [HttpPost("{collection}/upsert")]
        public async Task<IActionResult> Upsert(string collection)
        {
            using var sr = new StreamReader(Request.Body, Encoding.UTF8);
            var stringBody = await sr.ReadToEndAsync();
            var document = JsonSerializer.Deserialize(stringBody);
            storageService.Upsert(collection, document);
            return Accepted();
        }
    }
}