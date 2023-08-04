using System;
using System.Linq;
using maxbl4.Race.Logic.CheckpointService;
using maxbl4.Race.Logic.CheckpointService.Model;
using Microsoft.AspNetCore.Mvc;

namespace maxbl4.Race.DataService.Controllers
{
    [ApiController]
    [Route("options")]
    public class OptionsController : ControllerBase
    {
        private readonly CheckpointRepository checkpointRepository;

        public OptionsController(CheckpointRepository checkpointRepository)
        {
            this.checkpointRepository = checkpointRepository;
        }

        [HttpGet]
        public RfidOptions Get()
        {
            return checkpointRepository.GetRfidOptions();
        }

        [HttpGet("{property}")]
        public object Get(string property)
        {
            var opts = checkpointRepository.GetRfidOptions();
            var prop = opts.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
                return NotFound();
            var value = prop.GetValue(opts);
            if (value is string)
                return $"\"{value}\"";
            return value;
        }

        [HttpPost("{property}")]
        [HttpPut("{property}")]
        public object Put(string property, [FromBody] object newValue)
        {
            var opts = checkpointRepository.GetRfidOptions();
            var prop = opts.GetType().GetProperties()
                .FirstOrDefault(x => x.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
                return NotFound();
            prop.SetValue(opts, Convert.ChangeType(newValue, prop.PropertyType));
            checkpointRepository.SetRfidOptions(opts);
            return Ok();
        }

        [HttpPost]
        [HttpPut]
        public void Put([FromBody] RfidOptions options)
        {
            checkpointRepository.SetRfidOptions(options);
        }

        [HttpDelete]
        public void Delete()
        {
            checkpointRepository.SetRfidOptions(RfidOptions.Default);
        }
    }
}