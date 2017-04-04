using System.Threading.Tasks;
using Common.Extensions;
using Common.Extensions.Security;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using GPSLogger.Interfaces;

namespace GPSLogger.Implementations
{
    public class CredentialImpl : ICredential
    {
        private readonly Delegates.GenerateSaltDelegateAsync _generateSaltAsync;
        private readonly Delegates.GenerateCredentialDelegateAsync _generateCredentialAsync;
        private readonly IMessageHandler<bool, Credential<string>> _messageHandler;

        public CredentialImpl(
            Delegates.GenerateSaltDelegateAsync generateSaltAsync,
            Delegates.GenerateCredentialDelegateAsync generateCredentialAsync,
            IMessageHandler<bool, Credential<string>> messageHandler
        )
        {
            _generateSaltAsync = generateSaltAsync;
            _generateCredentialAsync = generateCredentialAsync;
            _messageHandler = messageHandler;
        }

        public async Task<SignedMessage<Credential<string>>> ProduceFromRequestAsync(CredentialGetParameters request)
        {
            if (_generateCredentialAsync == null)
                return null;
            if (_generateSaltAsync == null)
                return null;
            if (_messageHandler == null)
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

            var response = await _messageHandler.CreateResponseAsync(
                signedRequest,
                async valid =>
                    await (await _generateCredentialAsync(await _generateSaltAsync()))
                        .ConvertAsync(bytes => Task.FromResult(bytes.ToHexString())
                        )
            );

            return response;
        }
    }
}
