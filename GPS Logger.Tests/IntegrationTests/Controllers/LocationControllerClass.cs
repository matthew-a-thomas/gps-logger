using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPS_Logger.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class LocationControllerClass
    {
        private const string Root = "/api/location";
        private readonly TestServer _server = Helpers.CreateServer();

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ReturnsJSON() => Helpers.AssertReturnsJSON(_server, Root);
    }
}
