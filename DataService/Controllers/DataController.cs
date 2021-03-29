using System;
using System.Collections.Generic;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController
    {
        private readonly StorageService storageService;

        public DataController(StorageService storageService)
        {
            this.storageService = storageService;
        }

        [HttpGet("event/{id}")]
        public ActionResult<EventDto> GetEvent(Guid id)
        {
            return storageService.GetEvent(id);
        }
        
        [HttpGet("event")]
        public ActionResult<List<EventDto>> ListEvents()
        {
            return storageService.ListEvents();
        }
        
        [HttpPost("event")]
        [HttpPut("event")]
        public Guid UpsertEvent(EventDto entity)
        {
            //var entity = BsonMapper.Global.Deserialize<EventDto>(JsonSerializer.Deserialize(body.ToString()));
            storageService.UpsertEvent(entity);
            return entity.Id;
        }
        
        [HttpDelete("event/{id}")]
        public void DeleteEvent(Guid id)
        {
            storageService.DeleteEvent(id);
        }
    }
}