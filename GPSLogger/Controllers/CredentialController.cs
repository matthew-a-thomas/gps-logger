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
        public async Task<SignedMessage<Credential<string>>> GetAsync(SignedMessage<bool> request)
        {
            if (_messageHandler == null)
                return null;
            if (_generateCredentialAsync == null)
                return null;
            if (_generateSaltAsync == null)
                return null;
            return await _messageHandler.CreateResponseAsync(
                request,
                async valid =>
                    await (await _generateCredentialAsync(await _generateSaltAsync()))
                    .ConvertAsync(bytes => new ValueTask<string>(bytes.ToHexString())
                )
            );
        }
    }
}
