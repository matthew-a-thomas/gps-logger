using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Extensions.Security;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("api/[controller]")]
    public class CredentialController : ControllerBase
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class GetParameters
        {
            public bool Contents { get; set; }
            public string HMAC { get; set; }
            public string ID { get; set; }
            public string Salt { get; set; }
            public long UnixTime { get; set; }
        }

        /// <summary>
        /// The number of bytes in a Credential's ID
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const int IDSize = 16;

        private readonly Delegates.GenerateSaltDelegateAsync _generateSaltAsync;
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialAsync;
        private readonly IMessageHandler<bool, Credential<string>> _messageHandler;

        public CredentialController(
            Delegates.GenerateSaltDelegateAsync generateSaltAsync,
            Delegates.GenerateCredentialDelegateAsync generateCredentialAsync,
            IMessageHandler<bool, Credential<string>> messageHandler
        )
        {
            _generateSaltAsync = generateSaltAsync;
            _generateCredentialAsync = generateCredentialAsync;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Creates a new credential.
        /// Note that HMAC'ing the response does nothing to hide the secret part of the returned credential from eavesdroppers.
        /// If you want to hide the response, then make sure you're using encryption
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<SignedMessage<Credential<string>>> GetAsync(GetParameters request)
        {
            if (_messageHandler == null)
                return null;
            if (_generateCredentialAsync == null)
                return null;
            if (_generateSaltAsync == null)
                return null;

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
                async valid =>
                    await (await _generateCredentialAsync(await _generateSaltAsync()))
                    .ConvertAsync(bytes => Task.FromResult(bytes.ToHexString())
                )
            );
        }
    }
}
