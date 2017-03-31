using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Common.Messages;
using Common.Security.Signing;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public class GetParameters
        {
            public string HMAC { get; set; }
            public string ID { get; set; }
            public string Salt { get; set; }
            public long UnixTime { get; set; }
            public bool Contents { get; set; }
        }

        private readonly IMessageHandler<bool, long> _messageHandler;
        
        public TimeController(
            IMessageHandler<bool, long> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Returns this server's current time in seconds since Epoch
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<SignedMessage<long>> GetAsync(GetParameters parameters)
        {
            var signedRequest = new SignedMessage<bool>
            {
                HMAC = parameters.HMAC,
                Message = new Message<bool>
                {
                    Contents = parameters.Contents,
                    ID = parameters.ID,
                    Salt = parameters.Salt,
                    UnixTime = parameters.UnixTime
                }
            };
            return await _messageHandler.CreateResponseAsync(
                signedRequest,
                valid => Task.FromResult(DateTimeOffset.Now.ToUnixTimeSeconds())
                );
        }
    }
}