using System;
using Common.Messages;
using Common.Security.Signing;
using Microsoft.AspNetCore.Mvc;

namespace GPS_Logger.Core.Controllers
{
    public class TimeController : Controller
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
        public SignedMessage<long> Get(SignedMessage<bool> request) => _messageHandler.CreateResponse(request, valid => DateTimeOffset.Now.ToUnixTimeSeconds());
    }
}