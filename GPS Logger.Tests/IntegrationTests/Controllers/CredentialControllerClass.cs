using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Extensions;
using Common.Extensions.Security;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GPS_Logger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class CredentialControllerClass
    {
        private const string Root = "/api/credential";
        private readonly TestServer _server = Helpers.CreateServer();

        /// <summary>
        /// Parses a returned JSON credential from the given server
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static SignedMessage<Credential<string>> GetSignedCredential(TestServer server)
        {
            var response = Helpers.Get(server, Root);
            var signedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);
            return signedCredential;
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsJSON() => Helpers.AssertReturnsJSON(_server, Root);

        [TestMethod]
        public void ReturnsCredential() => GetSignedCredential(_server);

        [TestMethod]
        public void SignsResponseWhenSendingSignedRequest()
        {
            // Get some credentials from the server
            var signedCredential = GetSignedCredential(_server);
            var credentialAsStrings = signedCredential.Contents;
            var credentialAsBytes = credentialAsStrings.Convert(ByteArrayExtensions.FromHexString);

            // Create a request that has been signed with those credentials
            var messageSerializer = new MessageSerializer<bool>(Serializer<bool>.CreatePassthroughSerializer());
            var signer = new Signer<SignedMessage<bool>, Message<bool>>(
                new HMACProvider(() => new byte[0]),
                messageSerializer,
                new MapperTranslator<Message<bool>, SignedMessage<bool>>()
                );
            var request = signer.Sign(
                new Message<bool>
                {
                    ID = credentialAsStrings.ID,
                    Salt = "aabbcc",
                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                },
                credentialAsBytes.Secret);

            // Create URL parameters from the signed request
            var parameters = Helpers.CreateUrlParametersFrom(request);

            // Ask the server for another set of credentials
            var response = Helpers.Get(_server, Root + "?" + string.Join("&", parameters));
            var secondSignedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);

            // Assert that none of the returned fields are empty
            Assert.IsTrue(
                Helpers.ReflectPropertiesOf<string>(secondSignedCredential)
                    .All(tuple => !string.IsNullOrWhiteSpace(tuple.Item2)),
                "At least some of the returned signed message's properties are null or empty");
        }
    }
}
