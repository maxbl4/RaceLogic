using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using maxbl4.Race.DataService.Services;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
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

        public async Task<List<EventDto>> LoadEventsFromBraaap()
        {
            var mainClient = new BraaapWeb.Client.MainClient("https://braaap.ru", new HttpClient());
            var events = await mainClient.EventsAsync("zrbTmslsBNIEpj1zMjL0eK8Et6Tt_ivO-toyPHLOdBA", null);
            return events.Select(x => new EventDto
            {
                Id = x.EventId,
                Name = x.Name,
                ChampionshipId = x.ChampionshipId,
                StartOfRegistration = x.StartOfRegistration.UtcDateTime,
                EndOfRegistration = x.EndOfRegistration.UtcDateTime
            }).ToList();
        }
    }
}