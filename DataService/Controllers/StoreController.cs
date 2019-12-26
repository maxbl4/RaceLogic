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
        
        [HttpGet("{collection}/single/{id}")]
        public IActionResult SingleGet(string collection, string id)
        {
            var bsonId = BsonIdUrlEncoder.Decode(id);
            var doc = storageService.Get<BsonDocument>(bsonId, collection);
            if (doc == null)
                return NotFound();
            return Content(JsonSerializer.Serialize(doc), "application/json", Encoding.UTF8);
        }
        
        [HttpPost("{collection}/single/{id?}")]
        [HttpPut("{collection}/single/{id?}")]
        public async Task<IActionResult> SinglePut(string collection, string id = null)
        {
            var doc = await DocumentFromRequest();
            if (!string.IsNullOrEmpty(id))
            {
                doc["_id"] = BsonIdUrlEncoder.Decode(id);
            }
            storageService.Upsert(collection, doc);
            return CreatedAtAction("SingleGet", new {collection, id = BsonIdUrlEncoder.Encode(doc["_id"])}, null);
        }
        
        [HttpGet("{collection}/search")]
        public IActionResult Search(string collection, string @where = null, string order = null, int limit = 50)
        {
            if (string.IsNullOrEmpty(@where))
                @where = "1 = 1";
            var result = storageService.Search(collection, @where, order, limit);
            return Content(JsonSerializer.Serialize(new BsonArray(result)), "application/json", Encoding.UTF8);
        }
        
        [HttpGet("{collection}/count")]
        public IActionResult Count(string collection, string @where)
        {
            if (string.IsNullOrEmpty(@where))
                @where = "1 = 1";
            return Ok(storageService.Count(collection, @where));
        }

        async Task<BsonDocument> DocumentFromRequest()
        {
            using var sr = new StreamReader(Request.Body, Encoding.UTF8);
            var stringBody = await sr.ReadToEndAsync();
            return JsonSerializer.Deserialize(stringBody).AsDocument;
        }
    }
}