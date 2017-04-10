using System.Threading.Tasks;
using System.Linq;
using Common.Security.Signing;
using GPSLogger.Interfaces;
using GPSLogger.Models;
using GPSLogger.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    /// <summary>
    /// Handles saving and storing Locations
    /// </summary>
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocation _location;
        private readonly IActionResultProducer _resultProducer;

        public LocationController(
            ILocation location,
            IActionResultProducer resultProducer)
        {
            _location = location;
            _resultProducer = resultProducer;
        }

        /// <summary>
        /// Gets all the locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(string id = "") => await _resultProducer.ProduceAsync(async () => (await _location.GetLocationsForAsync(id)).OrderByDescending(x => x.UnixTime));

        /// <summary>
        /// Posts a new location
        /// </summary>
        /// <param name="posted"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] SignedMessage<Location> posted) => await _resultProducer.ProduceAsync(async () => await _location.AddLocationAsync(posted));
    }
}
