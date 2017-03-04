using Common.Extensions;
using Common.Extensions.Security;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Microsoft.AspNetCore.Mvc;

namespace GPS_Logger.Core.Controllers
{
    public class CredentialController : Controller
    {
        /// <summary>
        /// The number of bytes in a Credential's ID
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const int IDSize = 16;
        
        private readonly Delegates.GenerateSaltDelegate _generateSalt;
        private readonly Delegates.GenerateCredentialDelegate _generateCredential;
        private readonly MessageHandler<bool, Credential<string>> _messageHandler;

        public CredentialController(
            Delegates.GenerateSaltDelegate generateSalt,
            Delegates.GenerateCredentialDelegate generateCredential,
            MessageHandler<bool, Credential<string>> messageHandler
            )
        {
            _generateSalt = generateSalt;
            _generateCredential = generateCredential;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Creates a new credential.
        /// Note that HMAC'ing the response does nothing to hide the secret part of the returned credential from eavesdroppers.
        /// If you want to hide the response, then make sure you're using encryption
        /// </summary>
        /// <returns></returns>
        public SignedMessage<Credential<string>> Get(SignedMessage<bool> request) => _messageHandler.CreateResponse(request, valid => _generateCredential(_generateSalt()).Convert(bytes => ByteArrayExtensions.ToHexString(bytes)));
    }
}
