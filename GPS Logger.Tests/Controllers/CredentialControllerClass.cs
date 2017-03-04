using System;
using System.Security.Cryptography;
using Common.Extensions;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;
using GPS_Logger.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPS_Logger.Tests.Controllers
{
    [TestClass]
    public class CredentialControllerClass
    {
        [TestClass]
        public class GetMethod
        {
            private readonly Delegates.HMACKeyProvider _hmacKeyProvider;
            private readonly HMACProvider _hmacProvider;
            private readonly Delegates.GenerateCredentialDelegate _generateCredentials;
            private readonly Signer<SignedMessage<bool>, Message<bool>> _signerBool;
            private readonly Serializer<Credential<string>> _credentialSerializer;
            private readonly Signer<SignedMessage<Credential<string>>, Message<Credential<string>>> _signerCredential;
            private readonly Func<SignedMessage<bool>, byte[]> _idExtractor;
            private readonly Validator<SignedMessage<Credential<string>>, Message<Credential<string>>> _validator;
            private readonly ITranslator<Message<Credential<string>>, SignedMessage<Credential<string>>> _translatorCredentialMessage;
            private readonly Delegates.RNGFactory _rngFactory;
            private readonly Delegates.GenerateSaltDelegate _saltGenerator;
            private readonly ITranslator<Message<bool>, SignedMessage<bool>> _translatorBoolMessage;

            public GetMethod()
            {
                _rngFactory = RandomNumberGenerator.Create;
                _saltGenerator = () =>
                {
                    using (var rng = _rngFactory())
                    {
                        return rng.GetBytes(16);
                    }
                };
                _hmacKeyProvider = () => new byte[0];
                _hmacProvider = new HMACProvider(
                    _hmacKeyProvider
                );
                _generateCredentials = id =>
                {
                    using (var hmac = _hmacProvider.Get())
                    {
                        return new Credential<byte[]>
                        {
                            ID = id.CreateClone(),
                            Secret = hmac.ComputeHash(id)
                        };
                    }
                };
                _translatorCredentialMessage = new MapperTranslator<Message<Credential<string>>, SignedMessage<Credential<string>>>();
                _translatorBoolMessage = new MapperTranslator<Message<bool>, SignedMessage<bool>>();
                _signerBool = new Signer
                    <SignedMessage<bool>, Message<bool>>(
                        _hmacProvider,
                        new MessageSerializer<bool>(Serializer<bool>.CreatePassthroughSerializer()),
                        _translatorBoolMessage
                    );
                _credentialSerializer = new Serializer<Credential<string>>();
                {
                    _credentialSerializer.EnqueueStep(x => x.ID);
                    _credentialSerializer.EnqueueStep(x => x.Secret);
                }
                _signerCredential = new Signer<SignedMessage<Credential<string>>, Message<Credential<string>>>(
                    _hmacProvider,
                    new MessageSerializer<Credential<string>>(_credentialSerializer),
                    _translatorCredentialMessage
                );
                _idExtractor =
                    message => ByteArrayExtensions.FromHexString(message.ID);
                _validator = new Validator<SignedMessage<Credential<string>>, Message<Credential<string>>>(
                    _generateCredentials,
                    _signerCredential,
                    x => true,
                    x => ByteArrayExtensions.FromHexString(x.ID)
                    );
            }

            [TestMethod]
            public void CreatesValidCredential()
            {
                var controller = CreateController();
                var credentials = controller.Get(null).Contents;
                
                var regeneratedCredentials = _generateCredentials(ByteArrayExtensions.FromHexString(credentials.ID));
                Assert.IsTrue(credentials.ID == regeneratedCredentials.ID.ToHexString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(credentials.ID));
            }

            [TestMethod]
            public void DoesNotSignResponseToInvalidRequest()
            {
                var controller = CreateController();
                var response = controller.Get(null);
                Assert.IsNull(response.HMAC);
            }

            [TestMethod]
            public void MakesValidResponseToValidRequest()
            {
                var controller = CreateController();
                var credentials = controller.Get(null).Contents;

                var signedRequest = _signerBool.Sign(new Message<bool>
                    {
                        Contents = true,
                        ID = credentials.ID,
                        Salt = _saltGenerator().ToHexString()
                    },
                    ByteArrayExtensions.FromHexString(credentials.Secret));

                var response = controller.Get(signedRequest);

                Assert.IsTrue(_validator.IsValid(response));
            }

            /// <summary>
            /// Produces a new controller
            /// </summary>
            /// <returns></returns>
            private CredentialController CreateController()
            {
                return new CredentialController(
                    _saltGenerator,
                    _generateCredentials,
                    new MessageHandler<bool, Credential<string>>(
                        new Validator
                            <SignedMessage<bool>, Message<bool>>(
                                _generateCredentials,
                                _signerBool,
                                message => true,
                                _idExtractor
                            ),
                        _generateCredentials,
                        _signerCredential,
                        _idExtractor,
                        _translatorCredentialMessage
                    )
                );
            }
        }
    }
}
