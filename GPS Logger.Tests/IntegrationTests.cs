using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GPS_Logger.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        // ReSharper disable once InconsistentNaming
        private static void AssertReturnsJSON(string url)
        {
            using (var client = CreateServer().CreateClient())
            {
                var responseTask = client.GetAsync(url);
                responseTask.Wait();
                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
                var responseStringTask = response.Content.ReadAsStringAsync();
                responseStringTask.Wait();
                var responseString = responseStringTask.Result;
                JsonConvert.DeserializeObject(responseString);
            }
        }

        private static TestServer CreateServer()
        {
            return new TestServer(
                new WebHostBuilder()
                .UseContentRoot("../../../../GPS Logger")
                .UseStartup<Startup>()
                );
        }

        [TestClass]
        public class CredentialControllerClass
        {
            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON("/api/credential");
        }

        [TestClass]
        public class LocationControllerClass
        {
            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON("/api/location");
        }

        [TestClass]
        public class TimeControllerClass
        {
            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public void ReturnsJSON() => AssertReturnsJSON("/api/time");
        }
    }
}
