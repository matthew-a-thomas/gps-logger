using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using Common.Extensions;
using GPSLogger.Models;
using Common.Serialization;
using Common.Security.Signing;
using Common.Extensions.Security;
using Common.Security;
using Common.Messages;

namespace GPSLogger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class LocationControllerClass
    {
        private static readonly ISerializer<Location> LocationSerializer = new Func<Serializer<Location>>(() =>
        {
            var result = new Serializer<Location>();
            result.EnqueueStep(x => x.Latitude);
            result.EnqueueStep(x => x.Longitude);
            return result;
        })();
        private const string Root = "/api/location";
        private readonly TestServer _server = Helpers.CreateServer();

        [TestMethod]
        public void CannotReplayPost()
        {
            var signedResponse = CredentialControllerClass.GetSignedCredential(_server);
            var credentialAsStrings = signedResponse.Contents;
            var credentialAsBytes = credentialAsStrings.Convert(ByteArrayExtensions.FromHexString);
            var messageSerializer = new MessageSerializer<Location>(LocationSerializer);

            var requestContents = new Location
            {
                Latitude = 0,
                Longitude = 0
            };
            var signer = new Signer<SignedMessage<Location>, Message<Location>>(
                new HMACProvider(() => new byte[0]),
                messageSerializer,
                new MapperTranslator<Message<Location>, SignedMessage<Location>>()
                );
            var request = signer.Sign(
                new Message<Location>
                {
                    Contents = requestContents,
                    ID = credentialAsStrings.ID,
                    Salt = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString(),
                    UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                },
                credentialAsBytes.Secret);

            // Make the request twice
            SignedMessage<bool> deserializedResponse = null;
            for (var iteration = 0; iteration < 2; ++iteration)
            {
                var response = Helpers.Post(_server, Root, request);
                deserializedResponse = JsonConvert.DeserializeObject<SignedMessage<bool>>(response);
            }

            // Assert that the result is not signed
            Helpers.AssertIsNotSigned(deserializedResponse);
        }

        [TestMethod]
        public void CanPost()
        {
            CredentialControllerClass
                .DoWithSignedRequest(
                _server,
                Root,
                new Location
                {
                    Latitude = -9,
                    Longitude = 10
                },
                LocationSerializer,
                (request, server) =>
                {
                    var response = Helpers.Post(server, Root, request);

                    return response;
                },
                JsonConvert.DeserializeObject<SignedMessage<bool>>,
                Helpers.AssertNoPropertiesAreNull // Assert that none of the returned properties are null
                );
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsJSON() => Helpers.AssertReturnsJSON(_server, Root);

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsEmptyArrayForAbsentID()
        {
            var response = Helpers.Get(_server, Root);
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(!enumerable.Any());
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsEmptyArrayForRandomID()
        {
            var response = Helpers.Get(_server, Root + "?id=" + Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString());
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(!enumerable.Any());
        }
    }
}
