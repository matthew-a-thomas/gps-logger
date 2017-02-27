using System.Web.Http;
using GPS_Logger.Models;
using GPS_Logger.Models.Messages;
using GPS_Logger.Security;
using GPS_Logger.Security.Signing;

namespace GPS_Logger.Controllers
{
    public class CredentialController : ApiController
    {
        /// <summary>
        /// The number of bytes in a Credential's ID
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const int IDSize = 16;
        
        private readonly Delegates.GenerateSaltDelegate _generateSalt;
        private readonly Delegates.GenerateCredentialDelegate _generateCredential;
        private readonly MessageHandler<bool, Credential> _messageHandler;

        public CredentialController(
            Delegates.GenerateSaltDelegate generateSalt,
            Delegates.GenerateCredentialDelegate generateCredential,
            MessageHandler<bool, Credential> messageHandler
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
        public SignedMessage<Credential> Get([FromUri] SignedMessage<bool> request) => _messageHandler.CreateResponse(request, valid => _generateCredential(_generateSalt()));
    }
}
