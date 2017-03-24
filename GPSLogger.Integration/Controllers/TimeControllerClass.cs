//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.TestHost;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Common.Serialization;
//using Newtonsoft.Json;
//using Common.Security.Signing;

//namespace GPSLogger.Tests.IntegrationTests.Controllers
//{
//    [TestClass]
//    public class TimeControllerClass
//    {
//        private const string Root = "/api/time";
//        private readonly TestServer _server;

//        public TimeControllerClass()
//        {
//            var task = Helpers.CreateServerAsync();
//            task.Wait();
//            _server = task.Result;
//        }

//        [TestMethod]
//        // ReSharper disable once InconsistentNaming
//        public async Task ReturnsJSON() => await Helpers.AssertReturnsJSONAsync(_server, Root);

//        [TestMethod]
//        public async Task DoesNotSignResponseToUnsignedRequest()
//        {
//            var response = await Helpers.GetAsync(_server, Root);
//            var deserialized = JsonConvert.DeserializeObject<SignedMessage<long>>(response);
//            Assert.IsNull(deserialized.HMAC);
//            Assert.IsNull(deserialized.ID);
//            Assert.IsNull(deserialized.Salt);
//        }

//        [TestMethod]
//        public async Task ReturnsCloseToCurrentTime()
//        {
//            var response = await Helpers.GetAsync(_server, Root);
//            var deserialized = JsonConvert.DeserializeObject<SignedMessage<long>>(response);
//            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
//            var absDifference = Math.Abs(deserialized.Contents - now);
//            Assert.IsTrue(absDifference < 2, "The returned time is at least two seconds off");
//        }

//        [TestMethod]
//        public async Task SignsResponseToSignedRequest() =>
//            await CredentialControllerClass.DoWithSignedGetAsync(
//                _server,
//                Root,
//                true,
//                Serializer<bool>.CreatePassthroughSerializer(),
//                response => Task.Run(() => JsonConvert.DeserializeObject<SignedMessage<long>>(response)),
//                Helpers.AssertNoPropertiesAreNullAsync
//                );
//    }
//}
