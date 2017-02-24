using System;
using System.Web.Http;
using GPS_Logger.Extensions;
using GPS_Logger.Extensions.Messages;
using GPS_Logger.Models;
using GPS_Logger.Security;
using GPS_Logger.Security.Messages;
using GPS_Logger.Serialization;

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
        private readonly ISerializer<Credential> _credentialSerializer;
        private readonly MessageHandler _messageHandler;

        public CredentialController(
            Delegates.GenerateSaltDelegate generateSalt,
            Delegates.GenerateCredentialDelegate generateCredential,
            ISerializer<Credential> credentialSerializer,
            MessageHandler messageHandler)
        {
            _generateSalt = generateSalt;
            _generateCredential = generateCredential;
            _credentialSerializer = credentialSerializer;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Creates a new credential
        /// </summary>
        /// <returns></returns>
        public MessageToClient<Credential> Get() => _messageHandler.CreateUnsignedResponse(_generateCredential(_generateSalt()), _credentialSerializer);
    }
}
