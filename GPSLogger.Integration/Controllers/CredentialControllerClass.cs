//using System;
//using System.Text;
//using System.Threading.Tasks;
//using Common.Extensions;
//using Common.Extensions.Security;
//using Common.Messages;
//using Common.Security;
//using Common.Security.Signing;
//using Common.Serialization;
//using Microsoft.AspNetCore.TestHost;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;

//namespace GPSLogger.Tests.IntegrationTests.Controllers
//{
//    [TestClass]
//    public class CredentialControllerClass
//    {
//        private const string Root = "/api/credential";
//        private readonly TestServer _server;

//        public CredentialControllerClass()
//        {
//            var task = Helpers.CreateServerAsync();
//            task.Wait();
//            _server = task.Result;
//        }

//        public static async Task DoWithSignedGetAsync<TRequest, TResponse>(
//            TestServer server,
//            string requestRoot,
//            TRequest requestContents,
//            ISerializer<TRequest> serializer,
//            Func<string, Task<TResponse>> deserializeResponse,
//            Func<TResponse, Task> handleResponse)
//            => await DoWithSignedRequestAsync(
//                server,
//                requestRoot,
//                requestContents,
//                serializer,
//                // ReSharper disable once InconsistentNaming
//                async (request, _server) =>
//                {
//                    // Create URL parameters from the signed request
//                    var parameters = await Helpers.CreateUrlParametersFromAsync(request);

//                    // Ask the server for another set of credentials
//                    var response = await Helpers.GetAsync(_server, requestRoot + "?" + string.Join("&", parameters));
//                    return response;
//                },
//                deserializeResponse,
//                handleResponse);

//        /// <summary>
//        /// Performs an action on a signed response from 
//        /// </summary>
//        /// <typeparam name="TRequest"></typeparam>
//        /// <typeparam name="TResponse"></typeparam>
//        public static async Task DoWithSignedRequestAsync<TRequest, TResponse>(
//            TestServer server,
//            string requestRoot,
//            TRequest requestContents,
//            ISerializer<TRequest> serializer,
//            Func<SignedMessage<TRequest>, TestServer, Task<string>> performRequestAsync,
//            Func<string, Task<TResponse>> deserializeResponseAsync,
//            Func<TResponse, Task> handleResponseAsync)
//        {
//            // Get some credentials from the server
//            var signedCredential = await GetSignedCredentialAsync(server);
//            var credentialAsStrings = signedCredential.Contents;
//            var credentialAsBytes = await credentialAsStrings.ConvertAsync(ByteArrayExtensions.FromHexStringAsync);

//            // Create a request that has been signed with those credentials
//            var messageSerializer = new MessageSerializer<TRequest>(serializer);
//            var signer = new Signer<SignedMessage<TRequest>, Message<TRequest>>(
//                new HMACProvider(() => ValueTask(new byte[0])),
//                messageSerializer,
//                new MapperTranslator<Message<TRequest>, SignedMessage<TRequest>>()
//                );
//            var request = await signer.SignAsync(
//                new Message<TRequest>
//                {
//                    Contents = requestContents,
//                    ID = credentialAsStrings.ID,
//                    Salt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString(),
//                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
//                },
//                credentialAsBytes.Secret);

//            var responseString = await performRequestAsync(request, server);
//            var deserializedResponse = await deserializeResponseAsync(responseString);

//            // Perform the given action on the deserializedResponse
//            await handleResponseAsync(deserializedResponse);
//        }

//        /// <summary>
//        /// Parses a returned JSON credential from the given server
//        /// </summary>
//        /// <param name="server"></param>
//        /// <returns></returns>
//        public static async Task<SignedMessage<Credential<string>>> GetSignedCredentialAsync(TestServer server)
//        {
//            var response = await Helpers.GetAsync(server, Root);
//            var signedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);
//            return signedCredential;
//        }

//        [TestMethod]
//        // ReSharper disable once InconsistentNaming
//        public async Task ReturnsJSON() => await Helpers.AssertReturnsJSONAsync(_server, Root);

//        [TestMethod]
//        public async Task ReturnsValidCredential()
//        {
//            var credential = await GetSignedCredentialAsync(_server);
//            Assert.IsFalse(string.IsNullOrWhiteSpace(credential.Contents.ID));
//            Assert.IsFalse(string.IsNullOrWhiteSpace(credential.Contents.Secret));
//        }

//        [TestMethod]
//        public async Task DoesNotSignResponseWhenSendingUnsignedRequest()
//        {
//            // Get some credentials from the server
//            var signedCredential = await GetSignedCredentialAsync(_server);
//            var credentialAsStrings = signedCredential.Contents;

//            // Create a request that has been signed with those credentials
//            var request =
//                new Message<bool>
//                {
//                    ID = credentialAsStrings.ID,
//                    Salt = "aabbcc",
//                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
//                };

//            // Create URL parameters from the signed request
//            var parameters = Helpers.CreateUrlParametersFromAsync(request);

//            // Ask the server for another set of credentials
//            var response = await Helpers.GetAsync(_server, Root + "?" + string.Join("&", parameters));
//            var secondSignedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);

//            // Assert that the relevant fields are null
//            Assert.IsNull(secondSignedCredential.HMAC);
//            Assert.IsNull(secondSignedCredential.ID);
//            Assert.IsNull(secondSignedCredential.Salt);
//        }

//        [TestMethod]
//        public async Task SignsResponseWhenSendingSignedRequest()
//        {
//            // Assert that none of the returned fields are empty
//            await DoWithSignedGetAsync(
//                _server,
//                Root,
//                true,
//                Serializer<bool>.CreatePassthroughSerializer(),
//                response => Task.Run(() => JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response)),
//                Helpers.AssertNoPropertiesAreNullAsync);
//        }
//    }
//}
