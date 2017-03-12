using Common.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using static GPS_Logger.Controllers.HMACKeyController;

namespace GPS_Logger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class HMACKeyControllerClass
    {
        private const string Root = "/api/hmackey";

        private static TestServer PostKeyAndReturnServer()
        {
            var parameters = new PostParameters
            {
                NewKey = Encoding.ASCII.GetBytes("Hello world, this is your new key").ToHexString()
            };
            var server = Helpers.CreateServer();
            var response = Helpers.Post(server, Root, parameters);
            Assert.IsTrue(string.IsNullOrWhiteSpace(response));
            return server;
        }

        [TestMethod]
        public void CanPostKey()
        {
            PostKeyAndReturnServer();
        }
        
        [TestMethod]
        public void ReturnsFalseAtFirst()
        {
            var server = Helpers.CreateServer();
            var response = Helpers.Get(server, Root);
            var boolean = bool.Parse(response);
            Assert.IsFalse(boolean);
        }

        [TestMethod]
        public void ReturnsTrueAfterPostingKey()
        {
            var server = PostKeyAndReturnServer();
            var response = Helpers.Get(server, Root);
            var boolean = bool.Parse(response);
            Assert.IsTrue(boolean);
        }
    }
}
