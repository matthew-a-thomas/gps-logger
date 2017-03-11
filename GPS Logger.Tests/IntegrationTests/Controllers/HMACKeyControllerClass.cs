using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GPS_Logger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class HMACKeyControllerClass
    {
        private const string Root = "/api/hmackey";
        private readonly TestServer _server = Helpers.CreateServer();

        [TestMethod]
        public void ReturnsBoolean()
        {
            var response = Helpers.Get(_server, Root);
            bool.Parse(response);
        }
    }
}
