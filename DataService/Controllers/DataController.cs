using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.UpstreamData;
using maxbl4.Race.Logic.WebModel;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController: ControllerBase
    {
        private readonly IUpstreamDataSyncService syncService;
        private readonly IUpstreamDataRepository upstreamRepository;
        private readonly IEventRepository eventRepository;
        private readonly ITimingSessionService timingSessionService;

        public DataController(IUpstreamDataSyncService syncService, 
            IUpstreamDataRepository upstreamRepository, 
            IEventRepository eventRepository,
            ITimingSessionService timingSessionService)
        {
            this.syncService = syncService;
            this.upstreamRepository = upstreamRepository;
            this.eventRepository = eventRepository;
            this.timingSessionService = timingSessionService;
        }

        [HttpGet("event")]
        public ActionResult<EventDto> GetEvent(Id<EventDto> id)
        {
            return eventRepository.GetWithUpstream(id);
        }
        
        [HttpGet("sessions")]
        public ActionResult<List<SessionDto>> ListSessions(Id<EventDto> id)
        {
            return eventRepository.ListSessions(id).ToList();
        }
        
        [HttpGet("session")]
        public ActionResult<SessionDto> GetSession(Id<SessionDto> id)
        {
            return eventRepository.GetWithUpstream(id);
        }
        
        [HttpGet("timing-sessions")]
        public ActionResult<List<TimingSessionDto>> ListTimingSessions(Id<SessionDto> id)
        {
            return eventRepository.ListTimingSessions(id).ToList();
        }
        
        [HttpGet("timing-session")]
        public ActionResult<TimingSessionDto> GetTimingSession(Id<TimingSessionDto> id)
        {
            return eventRepository.GetWithUpstream(id);
        }
        
        [HttpDelete("timing-session")]
        public ActionResult DeleteTimingSession(Id<TimingSessionDto> id)
        {
            timingSessionService.StopSession(id);
            eventRepository.StorageService.Delete(id);
            return Ok();
        }
        
        [HttpPut("timing-session-start")]
        public ActionResult<Id<TimingSessionDto>> StartNewTimingSession([FromBody]TimingSessionDto timingSessionDto)
        {
            return timingSessionService.StartNewSession(timingSessionDto.Name, timingSessionDto.SessionId);
        }
        
        [HttpPost("timing-session-stop")]
        public ActionResult StopTimingSession(Id<TimingSessionDto> id)
        {
            timingSessionService.StopSession(id);
            return Ok();
        }
        
        [HttpPost("timing-session-resume")]
        public ActionResult ResumeTimingSession(Id<TimingSessionDto> id)
        {
            timingSessionService.ResumeSession(id);
            return Ok();
        }
        
        [HttpGet("timing-session-rider-info")]
        public ActionResult<List<RiderEventInfoDto>> ListRiderEventInfo(Id<TimingSessionDto> timingSessionId)
        {
            return eventRepository.ListRiderEventInfo(timingSessionId);
        }
        
        [HttpGet("timing-session-rating")]
        public ActionResult<TimingSessionUpdate> GetTimingSessionRating(Id<TimingSessionDto> timingSessionId)
        {
            return timingSessionService.GetTimingSessionRating(timingSessionId);
        }
        
        [HttpGet("active-timing-session-rating")]
        public ActionResult<List<TimingSessionDto>> ListActiveTimingSessions()
        {
            return timingSessionService.ListActiveTimingSessions();
        }
        
        [HttpPost("upstream/purge")]
        public ActionResult PurgeUpstreamData()
        {
            upstreamRepository.PurgeExistingData();
            return Ok();
        }

        [HttpPost("upstream")]
        public async Task<bool> DownloadUpstreamData(bool forceFullSync = false)
        {
            return await syncService.Download(forceFullSync);
        }
        
        [HttpGet("series")]
        public ActionResult<List<SeriesDto>> ListSeries()
        {
            return upstreamRepository.ListSeries().ToList();
        }
        
        [HttpGet("championships")]
        public ActionResult<List<ChampionshipDto>> ListChampionships([FromQuery]Id<SeriesDto> value)
        {
            return upstreamRepository.ListChampionships(value).ToList();
        }
        
        [HttpGet("classes")]
        public ActionResult<List<ClassDto>> ListClasses([FromQuery]Id<ChampionshipDto> value)
        {
            return upstreamRepository.ListClasses(value).ToList();
        }
        
        [HttpGet("events")]
        public ActionResult<List<EventDto>> ListEvents([FromQuery]Id<ChampionshipDto> value)
        {
            return upstreamRepository.ListEvents(value).ToList();
        }
    }
}