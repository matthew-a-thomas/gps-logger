using System;
using System.Web.Http;
using GPS_Logger.Security.Messages;

namespace GPS_Logger.Controllers
{
    public class TimeController : ApiController
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
        public SignedMessage<long> Get([FromUri] SignedMessage<bool> request) => _messageHandler.CreateResponse(request, valid => DateTimeOffset.Now.ToUnixTimeSeconds());
    }
}