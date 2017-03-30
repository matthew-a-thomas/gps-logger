using System;
using System.Threading.Tasks;
using Common.Security;
using Common.Security.Signing;

namespace Common.Messages
{
    /// <summary>
    /// Helps create responses to requests
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MessageHandler<TRequest, TResponse> : IMessageHandler<TRequest, TResponse>
    {
        private readonly Validator<TRequest> _validator;
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialAsync;
        private readonly Signer<TResponse> _signer;
        private readonly Validator<TRequest>.DeriveIDFromThingDelegateAsync _idExtractorAsync;

        public MessageHandler(
            Validator<TRequest> validator,
            Delegates.GenerateCredentialDelegateAsync generateCredentialAsync,
            Signer<TResponse> signer,
            Validator<TRequest>.DeriveIDFromThingDelegateAsync idExtractorAsync
            )
        {
            _validator = validator;
            _generateCredentialAsync = generateCredentialAsync;
            _signer = signer;
            _idExtractorAsync = idExtractorAsync;
        }
        
        public async ValueTask<SignedMessage<TResponse>> CreateResponseAsync(
            SignedMessage<TRequest> request,
            Func<bool, ValueTask<TResponse>> contentGeneratorAsync
            )
        {
            // Figure out if the request is valid
            var isValid = request != null && await _validator.IsValidAsync(request);

            // Generate a response based on that
            var response = new Message<TResponse>
            {
                Contents = await contentGeneratorAsync(isValid),
                ID = isValid ? request.Message?.ID : null,
                Salt = isValid ? request.Message?.Salt : null,
                UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            if (!isValid)
                return new SignedMessage<TResponse> { Message = response }; // The request wasn't valid, so what would be HMAC our response with? There's no client that would be able to verify it because our HMAC key is supposed to be a secret

            // Derive the client's secret
            var derivedCredential = await _generateCredentialAsync(await _idExtractorAsync(request.Message));

            // Sign the response with the client's secret
            var signedResponse = await _signer.SignAsync(response, derivedCredential.Secret);

            return signedResponse;
        }
    }
}