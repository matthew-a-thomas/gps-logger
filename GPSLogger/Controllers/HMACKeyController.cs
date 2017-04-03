using System.Threading.Tasks;
using GPSLogger.Interfaces;
using GPSLogger.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    /// <summary>
    /// Controller for the HMAC key, which is stored in persisted storage
    /// </summary>
    [Route("api/[controller]")]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyController : ControllerBase
    {
        private readonly IHMACKey _hmacKey;
        private readonly IActionResultProducer _resultProducer;

        public HMACKeyController(
            IHMACKey hmacKey,
            IActionResultProducer resultProducer)
        {
            _hmacKey = hmacKey;
            _resultProducer = resultProducer;
        }

        /// <summary>
        /// Returns a boolean indicating whether the HMAC key has been set
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        // ReSharper disable once MemberCanBePrivate.Global
        public async Task<IActionResult> GetAsync() => await _resultProducer.ProduceAsync(_hmacKey.IsSetAsync);

        /// <summary>
        /// Sets the HMAC key if it hasn't already been set
        /// </summary>
        [HttpPost]
        // ReSharper disable once UnusedMember.Global
        public async Task<IActionResult> PostAsync([FromBody] HMACPostParameters parameters) => await _resultProducer.ProduceAsync(async () => await _hmacKey.SetAsync(parameters));
    }
}
