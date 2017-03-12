using System;
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

        public static void DoWithSignedGet<TRequest, TResponse>(
            TestServer server,
            string requestRoot,
            TRequest requestContents,
            ISerializer<TRequest> serializer,
            Func<string, TResponse> deserializeResponse,
            Action<TResponse> handleResponse)
            => DoWithSignedRequest(
                server,
                requestRoot,
                requestContents,
                serializer,
                (request, _server) =>
                {
                    // Create URL parameters from the signed request
                    var parameters = Helpers.CreateUrlParametersFrom(request);

                    // Ask the server for another set of credentials
                    var response = Helpers.Get(_server, requestRoot + "?" + string.Join("&", parameters));
                    return response;
                },
                deserializeResponse,
                handleResponse);

        /// <summary>
        /// Performs an action on a signed response from 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="server"></param>
        /// <param name="requestRoot"></param>
        /// <param name="serializer"></param>
        /// <param name="handleResponse"></param>
        public static void DoWithSignedRequest<TRequest, TResponse>(
            TestServer server,
            string requestRoot,
            TRequest requestContents,
            ISerializer<TRequest> serializer,
            Func<SignedMessage<TRequest>, TestServer, string> performRequest,
            Func<string, TResponse> deserializeResponse,
            Action<TResponse> handleResponse)
        {
            // Get some credentials from the server
            var signedCredential = GetSignedCredential(server);
            var credentialAsStrings = signedCredential.Contents;
            var credentialAsBytes = credentialAsStrings.Convert(ByteArrayExtensions.FromHexString);

            // Create a request that has been signed with those credentials
            var messageSerializer = new MessageSerializer<TRequest>(serializer);
            var signer = new Signer<SignedMessage<TRequest>, Message<TRequest>>(
                new HMACProvider(() => new byte[0]),
                messageSerializer,
                new MapperTranslator<Message<TRequest>, SignedMessage<TRequest>>()
                );
            var request = signer.Sign(
                new Message<TRequest>
                {
                    Contents = requestContents,
                    ID = credentialAsStrings.ID,
                    Salt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString(),
                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                },
                credentialAsBytes.Secret);

            var responseString = performRequest(request, server);
            var deserializedResponse = deserializeResponse(responseString);

            // Perform the given action on the deserializedResponse
            handleResponse(deserializedResponse);
        }

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
        public void ReturnsValidCredential()
        {
            var credential = GetSignedCredential(_server);
            Assert.IsFalse(string.IsNullOrWhiteSpace(credential.Contents.ID));
            Assert.IsFalse(string.IsNullOrWhiteSpace(credential.Contents.Secret));
        }

        [TestMethod]
        public void DoesNotSignResponseWhenSendingUnsignedRequest()
        {
            // Get some credentials from the server
            var signedCredential = GetSignedCredential(_server);
            var credentialAsStrings = signedCredential.Contents;
            var credentialAsBytes = credentialAsStrings.Convert(ByteArrayExtensions.FromHexString);

            // Create a request that has been signed with those credentials
            var request =
                new Message<bool>
                {
                    ID = credentialAsStrings.ID,
                    Salt = "aabbcc",
                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

            // Create URL parameters from the signed request
            var parameters = Helpers.CreateUrlParametersFrom(request);

            // Ask the server for another set of credentials
            var response = Helpers.Get(_server, Root + "?" + string.Join("&", parameters));
            var secondSignedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);

            // Assert that the relevant fields are null
            Assert.IsNull(secondSignedCredential.HMAC);
            Assert.IsNull(secondSignedCredential.ID);
            Assert.IsNull(secondSignedCredential.Salt);
        }

        [TestMethod]
        public void SignsResponseWhenSendingSignedRequest()
        {
            // Assert that none of the returned fields are empty
            DoWithSignedGet(
                _server,
                Root,
                true,
                Serializer<bool>.CreatePassthroughSerializer(),
                response => JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response),
                signedResponse => Helpers.AssertNoPropertiesAreNull(signedResponse));
        }
    }
}
