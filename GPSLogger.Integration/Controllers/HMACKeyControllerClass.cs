//using Common.Extensions;
//using Microsoft.AspNetCore.TestHost;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Text;
//using System.Threading.Tasks;
//using static GPSLogger.Controllers.HMACKeyController;

//namespace GPSLogger.Tests.IntegrationTests.Controllers
//{
//    [TestClass]
//    // ReSharper disable once InconsistentNaming
//    public class HMACKeyControllerClass
//    {
//        private const string Root = "/api/hmackey";

//        private static async Task<TestServer> PostKeyAndReturnServerAsync()
//        {
//            var parameters = new PostParameters
//            {
//                NewKey = Encoding.ASCII.GetBytes("Hello world, this is your new key").ToHexString()
//            };
//            var server = await Helpers.CreateServerAsync();
//            var response = await Helpers.PostAsync(server, Root, parameters);
//            Assert.IsTrue(string.IsNullOrWhiteSpace(response));
//            return server;
//        }

//        [TestMethod]
//        public async Task CanPostKey()
//        {
//            await PostKeyAndReturnServerAsync();
//        }
        
//        [TestMethod]
//        public async Task ReturnsFalseAtFirst()
//        {
//            var server = await Helpers.CreateServerAsync();
//            var response = await Helpers.GetAsync(server, Root);
//            var boolean = bool.Parse(response);
//            Assert.IsFalse(boolean);
//        }

//        [TestMethod]
//        public async Task ReturnsTrueAfterPostingKey()
//        {
//            var server = await PostKeyAndReturnServerAsync();
//            var response = await Helpers.GetAsync(server, Root);
//            var boolean = bool.Parse(response);
//            Assert.IsTrue(boolean);
//        }
//    }
//}
