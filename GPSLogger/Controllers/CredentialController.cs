using System.Threading.Tasks;
using GPSLogger.Interfaces;
using GPSLogger.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class CredentialController : ControllerBase
    {
        private readonly ICredential _credential;
        private readonly IActionResultProducer _resultProducer;

        public CredentialController(
            ICredential credential,
            IActionResultProducer resultProducer)
        {
            _credential = credential;
            _resultProducer = resultProducer;
        }

        /// <summary>
        /// Creates a new credential.
        /// Note that HMAC'ing the response does nothing to hide the secret part of the returned credential from eavesdroppers.
        /// If you want to hide the response, then make sure you're using encryption
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(CredentialGetParameters request) => await _resultProducer.ProduceAsync(async () => await _credential.ProduceFromRequestAsync(request));
    }
}
