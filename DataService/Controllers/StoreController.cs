using System.Text;
using LiteDB;
using maxbl4.Race.DataService.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("store")]
    public class StoreController : ControllerBase
    {
        private readonly DataServiceRepository repository;

        public StoreController(DataServiceRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("")]
        public IActionResult ShowHelp()
        {
            return Ok(new
            {
                Description = "This is a simple storage API",
                Examples = new object[]
                {
                    new
                    {
                        Query = new {Uri = "GET /store/Riders/single/1"},
                        Returns = new
                            {Code = 200, Data = new {Id = 1, Name = "Rider Name", OtherRiderAttribute = "123"}}
                    },
                    new
                    {
                        Query = new
                        {
                            Uri = "PUT|POST /store/Riders/single",
                            Body = new {Name = "New Rider Name", Some = "More data"}
                        },
                        Returns = new
                        {
                            Code = 201, Headers = new {Location = "/store/Riders/single/5"},
                            Comment = "Location header has id of new entity"
                        }
                    },
                    new
                    {
                        Query = new
                        {
                            Uri = "PUT|POST /store/Riders/single/1", Comment = "Explicit new id can be specified"
                        }
                    }
                }
            });
        }

        [HttpGet("{collection}/single/{id}")]
        public IActionResult SingleGet(string collection, string id)
        {
            var bsonId = BsonIdUrlEncoder.Decode(id);
            var doc = repository.Get<BsonDocument>(bsonId, collection);
            if (doc == null)
                return NotFound();
            return Content(JsonSerializer.Serialize(doc), "application/json", Encoding.UTF8);
        }

        [HttpDelete("{collection}/single/{id}")]
        public IActionResult SingleDelete(string collection, string id)
        {
            var bsonId = BsonIdUrlEncoder.Decode(id);
            if (!repository.Delete<BsonDocument>(bsonId, collection))
                return NotFound();
            return Ok();
        }

        [HttpPost("{collection}/single/{id?}")]
        [HttpPut("{collection}/single/{id?}")]
        public IActionResult SinglePut(string collection, [FromBody]JObject body, string id = null)
        {
            var doc = JsonSerializer.Deserialize(body.ToString()).AsDocument;
            if (!string.IsNullOrEmpty(id)) doc["_id"] = BsonIdUrlEncoder.Decode(id);
            repository.Upsert(collection, doc);
            return CreatedAtAction("SingleGet", new {collection, id = BsonIdUrlEncoder.Encode(doc["_id"])}, null);
        }

        [HttpGet("{collection}/search")]
        public IActionResult Search(string collection, string where = null, string order = null, int limit = 50)
        {
            if (string.IsNullOrEmpty(where))
                where = "1 = 1";
            var result = repository.Search(collection, where, order, limit);
            return Content(JsonSerializer.Serialize(new BsonArray(result)), "application/json", Encoding.UTF8);
        }

        [HttpGet("{collection}/count")]
        public IActionResult Count(string collection, string where)
        {
            if (string.IsNullOrEmpty(where))
                where = "1 = 1";
            return Ok(repository.Count(collection, where));
        }
    }
}