using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Extensions.Security;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GPS_Logger.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        /// <summary>
        /// Asserts that what comes back from the given URL is valid JSON
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        // ReSharper disable once InconsistentNaming
        private static void AssertReturnsJSON(TestServer server, string url)
        {
            JsonConvert.DeserializeObject(Get(server, url));
        }
    
        /// <summary>
        /// Creates a new GPS Logger server
        /// </summary>
        /// <returns></returns>
        private static TestServer CreateServer()
        {
            return new TestServer(
                new WebHostBuilder()
                .UseContentRoot("../../../../GPS Logger")
                .UseStartup<Startup>()
                );
        }

        /// <summary>
        /// Recursively reflects the given object, pulling out all public instance properties as "name=value" pairs
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static IEnumerable<string> CreateUrlParametersFrom(object o)
            => ReflectPropertiesOf<string>(o).Select(tuple => tuple.Item1.Name + "=" + WebUtility.UrlEncode(tuple.Item2));

        /// <summary>
        /// Returns the string the came back from getting the given URL from the given server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string Get(TestServer server, string url)
        {
            using (var client = server.CreateClient())
            {
                var responseTask = client.GetAsync(url);
                responseTask.Wait();
                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
                var responseStringTask = response.Content.ReadAsStringAsync();
                responseStringTask.Wait();
                var responseString = responseStringTask.Result;
                return responseString;
            }
        }

        private static IEnumerable<Tuple<PropertyInfo, T>> ReflectPropertiesOf<T>(object o)
        {
            foreach (var property in
                o
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                var propertyValue = property.GetValue(o);
                var convertedValue = default(T);
                var success = false;
                try
                {
                    convertedValue = (T)Convert.ChangeType(propertyValue, typeof(T));
                    success = true;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }

                if (success)
                    yield return Tuple.Create(property, convertedValue);
                else
                    foreach (var thing in ReflectPropertiesOf<T>(propertyValue))
                        yield return thing;
            }
        }

        [TestClass]
        public class CredentialControllerClass
        {
            private const string Root = "/api/credential";
            private readonly TestServer _server = CreateServer();

            /// <summary>
            /// Parses a returned JSON credential from the given server
            /// </summary>
            /// <param name="server"></param>
            /// <returns></returns>
            public static SignedMessage<Credential<string>> GetSignedCredential(TestServer server)
            {
                var response = Get(server, Root);
                var signedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);
                return signedCredential;
            }

            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON(_server, Root);

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
                var parameters = CreateUrlParametersFrom(request);

                // Ask the server for another set of credentials
                var response = Get(_server, Root + "?" + string.Join("&", parameters));
                var secondSignedCredential = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(response);

                // Assert that none of the returned fields are empty
                Assert.IsTrue(
                    ReflectPropertiesOf<string>(secondSignedCredential)
                        .All(tuple => !string.IsNullOrWhiteSpace(tuple.Item2)),
                    "At least some of the returned signed message's properties are null or empty");
            }
        }

        [TestClass]
        public class LocationControllerClass
        {
            private const string Root = "/api/location";
            private readonly TestServer _server = CreateServer();

            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON(_server, Root);
        }

        [TestClass]
        public class TimeControllerClass
        {
            private const string Root = "/api/time";
            private readonly TestServer _server = CreateServer();

            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON(_server, Root);
        }
    }
}
