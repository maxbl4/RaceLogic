using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maxbl4.Race.Logic.EventModel.Runtime;
using maxbl4.Race.Logic.EventModel.Storage.Identifier;
using maxbl4.Race.Logic.EventModel.Storage.Model;
using maxbl4.Race.Logic.EventStorage.Storage;
using maxbl4.Race.Logic.UpstreamData;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController: ControllerBase
    {
        private readonly IUpstreamDataSyncService syncService;
        private readonly IUpstreamDataRepository upstreamRepository;
        private readonly IEventRepository eventRepository;
        private readonly IRecordingServiceRepository recordingRepository;
        private readonly ITimingSessionService timingSessionService;

        public DataController(IUpstreamDataSyncService syncService, 
            IUpstreamDataRepository upstreamRepository, 
            IEventRepository eventRepository,
            IRecordingServiceRepository recordingRepository,
            ITimingSessionService timingSessionService)
        {
            this.syncService = syncService;
            this.upstreamRepository = upstreamRepository;
            this.eventRepository = eventRepository;
            this.recordingRepository = recordingRepository;
            this.timingSessionService = timingSessionService;
        }

        [HttpGet("event")]
        public ActionResult<EventDto> GetEvent([FromQuery] Id<EventDto> value)
        {
            return eventRepository.GetWithUpstream(value);
        }
        
        [HttpGet("sessions")]
        public ActionResult<List<SessionDto>> ListSessions([FromQuery] Id<EventDto> value)
        {
            return eventRepository.ListSessions(value).ToList();
        }
        
        [HttpGet("session")]
        public ActionResult<SessionDto> GetSession([FromQuery] Id<SessionDto> value)
        {
            return eventRepository.GetWithUpstream(value);
        }
        
        [HttpGet("timing-sessions")]
        public ActionResult<List<TimingSessionDto>> ListTimingSessions([FromQuery] Id<SessionDto> value)
        {
            return eventRepository.ListTimingSessions(value).ToList();
        }
        
        [HttpGet("timing-session")]
        public ActionResult<TimingSessionDto> GetTimingSession([FromQuery] Id<TimingSessionDto> value)
        {
            return eventRepository.GetWithUpstream(value);
        }
        
        [HttpPost("timing-session-start")]
        public ActionResult<Id<TimingSessionDto>> StartTimingSession([FromQuery] Id<SessionDto> value)
        {
            var session = eventRepository.GetWithUpstream(value);
            var t = timingSessionService.CreateSession(session.Name, value);
            return t.Id;
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