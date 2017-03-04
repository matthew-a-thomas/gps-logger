using System;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;

namespace Common.Messages
{
    /// <summary>
    /// Helps create responses to requests
    /// </summary>
    public class MessageHandler<TRequest, TResponse>
    {
        private readonly Validator<SignedMessage<TRequest>, Message<TRequest>> _validator;
        private readonly Delegates.GenerateCredentialDelegate _generateCredential;
        private readonly Signer<SignedMessage<TResponse>, Message<TResponse>> _signer;
        private readonly Func<SignedMessage<TRequest>, byte[]> _idExtractor;
        private readonly ITranslator<Message<TResponse>, SignedMessage<TResponse>> _messageTranslator;

        public MessageHandler(
            Validator<SignedMessage<TRequest>, Message<TRequest>> validator,
            Delegates.GenerateCredentialDelegate generateCredential,
            Signer<SignedMessage<TResponse>, Message<TResponse>> signer,
            Func<SignedMessage<TRequest>, byte[]> idExtractor,
            ITranslator<Message<TResponse>, SignedMessage<TResponse>> messageTranslator
            )
        {
            _validator = validator;
            _generateCredential = generateCredential;
            _signer = signer;
            _idExtractor = idExtractor;
            _messageTranslator = messageTranslator;
        }
        
        public SignedMessage<TResponse> CreateResponse(
            SignedMessage<TRequest> request,
            Func<bool, TResponse> contentGenerator
            )
        {
            // Figure out if the request is valid
            var isValid = request != null && _validator.IsValid(request);

            // Generate a response based on that
            var response = new Message<TResponse>
            {
                Contents = contentGenerator(isValid),
                ID = isValid ? request?.ID : null,
                Salt = isValid ? request?.Salt : null,
                UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            if (!isValid)
                return _messageTranslator.Translate(response); // The request wasn't valid, so what would be HMAC our response with? There's no client that would be able to verify it because our HMAC key is supposed to be a secret

            // Derive the client's secret
            var derivedCredential = _generateCredential(_idExtractor(request));

            // Sign the response with the client's secret
            var signedResponse = _signer.Sign(response, derivedCredential.Secret);

            return signedResponse;
        }
    }
}