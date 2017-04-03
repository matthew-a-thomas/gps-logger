using System.Threading.Tasks;
using GPSLogger.Interfaces;
using GPSLogger.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        private readonly IActionResultProducer _resultProducer;
        private readonly ITime _time;

        public TimeController(
            IActionResultProducer resultProducer,
            ITime time)
        {
            _resultProducer = resultProducer;
            _time = time;
        }

        /// <summary>
        /// Returns this server's current time in seconds since Epoch
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(TimeGetParameters parameters) => await _resultProducer.ProduceAsync(async () => await _time.GetCurrentTimeAsync(parameters));
    }
}