using System;
using System.Threading.Tasks;
using Common.Messages;
using Common.Security.Signing;
using GPSLogger.Interfaces;

namespace GPSLogger.Implementations
{
    public class Time : ITime
    {
        private readonly IMessageHandler<bool, long> _messageHandler;

        public Time(IMessageHandler<bool, long> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public async Task<SignedMessage<long>> GetCurrentTimeAsync(TimeGetParameters request)
        {
            var signedRequest = new SignedMessage<bool>
            {
                HMAC = request?.HMAC,
                Message = new Message<bool>
                {
                    Contents = request?.Contents ?? false,
                    ID = request?.ID,
                    Salt = request?.Salt,
                    UnixTime = request?.UnixTime ?? 0
                }
            };
            return await _messageHandler.CreateResponseAsync(
                signedRequest,
                valid => Task.FromResult(DateTimeOffset.Now.ToUnixTimeSeconds())
            );
        }
    }
}
