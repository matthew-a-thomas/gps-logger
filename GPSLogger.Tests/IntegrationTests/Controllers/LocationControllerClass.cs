using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
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
            result.EnqueueStepAsync(x => Task.FromResult(x.Latitude));
            result.EnqueueStepAsync(x => Task.FromResult(x.Longitude));
            return result;
        })();
        private const string Root = "/api/location";
        private readonly TestServer _server;

        public LocationControllerClass()
        {
            var task = Helpers.CreateServerAsync();
            task.Wait();
            _server = task.Result;
        }

        [TestMethod]
        public async Task CannotReplayPost()
        {
            var signedResponse = await CredentialControllerClass.GetSignedCredentialAsync(_server);
            var credentialAsStrings = signedResponse.Contents;
            var credentialAsBytes = await credentialAsStrings.ConvertAsync(ByteArrayExtensions.FromHexStringAsync);
            var messageSerializer = new MessageSerializer<Location>(LocationSerializer);

            var requestContents = new Location
            {
                Latitude = 0,
                Longitude = 0
            };
            var signer = new Signer<SignedMessage<Location>, Message<Location>>(
                new HMACProvider(() => Task.FromResult(new byte[0])),
                messageSerializer,
                new MapperTranslator<Message<Location>, SignedMessage<Location>>()
                );
            var request = await signer.SignAsync(
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
                var response = await Helpers.PostAsync(_server, Root, request);
                deserializedResponse = JsonConvert.DeserializeObject<SignedMessage<bool>>(response);
            }

            // Assert that the result is not signed
            await Helpers.AssertIsNotSigned(deserializedResponse);
        }

        [TestMethod]
        public async Task CanPost()
        {
            await CredentialControllerClass
                .DoWithSignedRequestAsync(
                    _server,
                    Root,
                    new Location
                    {
                        Latitude = -9,
                        Longitude = 10
                    },
                    LocationSerializer,
                    async (request, server) =>
                    {
                        var response = await Helpers.PostAsync(server, Root, request);

                        return response;
                    },
                    _ => Task.Run(() => JsonConvert.DeserializeObject<SignedMessage<bool>>(_)),
                    Helpers.AssertNoPropertiesAreNullAsync // Assert that none of the returned properties are null
                );
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public async Task ReturnsJSON() => await Helpers.AssertReturnsJSONAsync(_server, Root);

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public async Task ReturnsEmptyArrayForAbsentID()
        {
            var response = await Helpers.GetAsync(_server, Root);
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(!enumerable.Any());
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public async Task ReturnsEmptyArrayForRandomID()
        {
            var response = await Helpers.GetAsync(_server, Root + "?id=" + Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString());
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(!enumerable.Any());
        }
    }
}
