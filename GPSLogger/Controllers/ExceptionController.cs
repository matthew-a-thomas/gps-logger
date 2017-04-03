using System;
using System.Threading.Tasks;
using GPSLogger.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class ExceptionController : ControllerBase
    {
        private readonly IActionResultProducer _resultProducer;

        public ExceptionController(IActionResultProducer resultProducer)
        {
            _resultProducer = resultProducer;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string message) => await _resultProducer.ProduceAsync(() => throw new Exception(message));
    }
}
