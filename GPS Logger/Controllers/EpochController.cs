using System;
using System.Web.Http;
using GPS_Logger.Security.Messages;
using GPS_Logger.Serialization;

namespace GPS_Logger.Controllers
{
    public class EpochController : ApiController
    {
        private readonly MessageHandler _messageHandler;
        
        public EpochController(
            MessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Returns this server's current time in seconds since Epoch
        /// </summary>
        /// <returns></returns>
        public MessageToClient<long> Get([FromUri] MessageFromClient<bool> request) => _messageHandler.CreateResponse(request, Serializer<bool>.CreatePassthroughSerializer(), valid => DateTimeOffset.Now.ToUnixTimeSeconds(), Serializer<long>.CreatePassthroughSerializer());
    }
}