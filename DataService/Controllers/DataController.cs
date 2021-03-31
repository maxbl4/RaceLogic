using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiteDB;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventStorage.Storage.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
        
        //[HttpPost("event/{id?}")]
        [HttpPut("event/{id?}")]
        public Guid UpsertEvent(Id<EventDto> id, EventDto entity)
        {
            if (id != Id<EventDto>.Empty)
                entity.Id = id;
            storageService.UpsertEvent(entity);
            return entity.Id;
        }
        
        [HttpDelete("event/{id}")]
        public void DeleteEvent(Id<EventDto> id)
        {
            storageService.DeleteEvent(id);
        }
    }
}