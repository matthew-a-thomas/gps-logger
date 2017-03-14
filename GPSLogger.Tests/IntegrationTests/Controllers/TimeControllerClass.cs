using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Serialization;
using Newtonsoft.Json;
using Common.Security.Signing;

namespace GPSLogger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class TimeControllerClass
    {
        private const string Root = "/api/time";
        private readonly TestServer _server = Helpers.CreateServer();

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsJSON() => Helpers.AssertReturnsJSON(_server, Root);

        [TestMethod]
        public void DoesNotSignResponseToUnsignedRequest()
        {
            var response = Helpers.Get(_server, Root);
            var deserialized = JsonConvert.DeserializeObject<SignedMessage<long>>(response);
            Assert.IsNull(deserialized.HMAC);
            Assert.IsNull(deserialized.ID);
            Assert.IsNull(deserialized.Salt);
        }

        [TestMethod]
        public void ReturnsCloseToCurrentTime()
        {
            var response = Helpers.Get(_server, Root);
            var deserialized = JsonConvert.DeserializeObject<SignedMessage<long>>(response);
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var absDifference = Math.Abs(deserialized.Contents - now);
            Assert.IsTrue(absDifference < 2, "The returned time is at least two seconds off");
        }

        [TestMethod]
        public void SignsResponseToSignedRequest() =>
            CredentialControllerClass.DoWithSignedGet(
                _server,
                Root,
                true,
                Serializer<bool>.CreatePassthroughSerializer(),
                response => JsonConvert.DeserializeObject<SignedMessage<long>>(response),
                signedResponse => Helpers.AssertNoPropertiesAreNull(signedResponse)
                );
    }
}
