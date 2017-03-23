using System;
using System.Threading.Tasks;
using Common.Messages;
using Common.Security.Signing;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        private readonly MessageHandler<bool, long> _messageHandler;
        
        public TimeController(
            MessageHandler<bool, long> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Returns this server's current time in seconds since Epoch
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<SignedMessage<long>> GetAsync(SignedMessage<bool> request) => await _messageHandler.CreateResponseAsync(request, valid => Task.FromResult(DateTimeOffset.Now.ToUnixTimeSeconds()));
    }
}