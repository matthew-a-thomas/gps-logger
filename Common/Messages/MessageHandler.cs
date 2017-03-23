using System;
using System.Threading.Tasks;
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
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialAsync;
        private readonly Signer<SignedMessage<TResponse>, Message<TResponse>> _signer;
        private readonly Validator<SignedMessage<TRequest>, Message<TRequest>>.DeriveIDFromThingDelegateAsync _idExtractorAsync;
        private readonly ITranslator<Message<TResponse>, SignedMessage<TResponse>> _messageTranslator;

        public MessageHandler(
            Validator<SignedMessage<TRequest>, Message<TRequest>> validator,
            Delegates.GenerateCredentialDelegateAsync generateCredentialAsync,
            Signer<SignedMessage<TResponse>, Message<TResponse>> signer,
            Validator<SignedMessage<TRequest>, Message<TRequest>>.DeriveIDFromThingDelegateAsync idExtractorAsync,
            ITranslator<Message<TResponse>, SignedMessage<TResponse>> messageTranslator
            )
        {
            _validator = validator;
            _generateCredentialAsync = generateCredentialAsync;
            _signer = signer;
            _idExtractorAsync = idExtractorAsync;
            _messageTranslator = messageTranslator;
        }
        
        public async Task<SignedMessage<TResponse>> CreateResponseAsync(
            SignedMessage<TRequest> request,
            Func<bool, Task<TResponse>> contentGeneratorAsync
            )
        {
            // Figure out if the request is valid
            var isValid = request != null && await _validator.IsValidAsync(request);

            // Generate a response based on that
            var response = new Message<TResponse>
            {
                Contents = await contentGeneratorAsync(isValid),
                ID = isValid ? request?.ID : null,
                Salt = isValid ? request?.Salt : null,
                UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            if (!isValid)
                return await _messageTranslator.TranslateAsync(response); // The request wasn't valid, so what would be HMAC our response with? There's no client that would be able to verify it because our HMAC key is supposed to be a secret

            // Derive the client's secret
            var derivedCredential = await _generateCredentialAsync(await _idExtractorAsync(request));

            // Sign the response with the client's secret
            var signedResponse = await _signer.SignAsync(response, derivedCredential.Secret);

            return signedResponse;
        }
    }
}