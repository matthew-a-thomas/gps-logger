using System;
using GPS_Logger.Extensions;
using GPS_Logger.Extensions.Security;
using GPS_Logger.Security.Messages.Signing;
using GPS_Logger.Serialization;

namespace GPS_Logger.Security.Messages
{
    /// <summary>
    /// Helps create responses to requests
    /// </summary>
    public class MessageHandler
    {
        private readonly MessageValidator _messageValidator;
        private readonly Delegates.GenerateSaltDelegate _generateSalt;
        private readonly Delegates.GenerateCredentialDelegate _generateCredential;
        private readonly MessageSigner _messageSigner;

        public MessageHandler(
            MessageValidator messageValidator,
            Delegates.GenerateSaltDelegate generateSalt,
            Delegates.GenerateCredentialDelegate generateCredential,
            MessageSigner messageSigner
            )
        {
            _messageValidator = messageValidator;
            _generateSalt = generateSalt;
            _generateCredential = generateCredential;
            _messageSigner = messageSigner;
        }

        /// <summary>
        /// Creates a response to the given request
        /// </summary>
        /// <typeparam name="TFromClient">The payload type of the request message</typeparam>
        /// <typeparam name="TToClient">The payload type of the response message</typeparam>
        /// <param name="request">The client's request</param>
        /// <param name="requestContentSerializer">Something that can serialize the request's payload</param>
        /// <param name="contentGenerator">Something that generates a response based on the request being valid or not</param>
        /// <param name="responseContentSerializer">Something that can serialize the response's payload</param>
        /// <returns></returns>
        public MessageToClient<TToClient> CreateResponse<TFromClient, TToClient>(MessageFromClient<TFromClient> request, ISerializer<TFromClient> requestContentSerializer, Func<bool, TToClient> contentGenerator, ISerializer<TToClient> responseContentSerializer)
        {
            // Figure out if the request is valid
            var isValid = request != null && _messageValidator.IsValid(request, requestContentSerializer);

            // Generate a response based on that
            var response = new MessageToClient<TToClient>
            {
                ClientSalt = isValid ? request.ClientSalt : null,
                ServerEpoch = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ServerSalt = _generateSalt().ToHexString(),
                Contents = contentGenerator(isValid)
            };

            if (!isValid)
                return response; // The request wasn't valid, so what would be HMAC our response with? There's no client that would be able to verify it because our HMAC key is supposed to be a secret

            // Derive the client's secret, and HMAC the response with that
            var derivedCredential = _generateCredential(ByteArrayExtensions.FromHexString(request.ID));
            _messageSigner.Sign(response, ByteArrayExtensions.FromHexString(derivedCredential.Secret), responseContentSerializer);

            return response;
        }
    }
}