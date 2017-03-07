using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;
using Common.Extensions;
using GPS_Logger.Models;
using Common.Serialization;
using Common.Security.Signing;

namespace GPS_Logger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class LocationControllerClass
    {
        private const string Root = "/api/location";
        private readonly TestServer _server = Helpers.CreateServer();

        [TestMethod]
        public void CanPost()
        {
            var locationSerializer = new Serializer<Location>();
            locationSerializer.EnqueueStep(x => x.Latitude);
            locationSerializer.EnqueueStep(x => x.Longitude);
            CredentialControllerClass
                .DoWithSignedRequest(
                _server,
                Root,
                new Location
                {
                    Latitude = -9,
                    Longitude = 10
                },
                locationSerializer,
                (request, server) =>
                {
                    var response = Helpers.Post(server, Root, Helpers.CreateUrlParametersFrom(request).ToArray());

                    return response;
                },
                response => JsonConvert.DeserializeObject<SignedMessage<bool>>(response),
                response => Helpers.AssertNoPropertiesAreNull(response) // Assert that none of the returned properties are null
                );
        }

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsJSON() => Helpers.AssertReturnsJSON(_server, Root);

        [TestMethod]
        public void ReturnsEmptyArrayForAbsentID()
        {
            var response = Helpers.Get(_server, Root);
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(enumerable.Count() == 0);
        }

        [TestMethod]
        public void ReturnsEmptyArrayForRandomID()
        {
            var response = Helpers.Get(_server, Root + "?id=" + Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()).ToHexString());
            var enumerable = JsonConvert.DeserializeObject<IEnumerable<object>>(response);
            Assert.IsTrue(enumerable.Count() == 0);
        }
    }
}
