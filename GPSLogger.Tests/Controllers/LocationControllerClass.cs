using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Messages;
using GPSLogger.Controllers;
using GPSLogger.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Controllers
{
    [TestClass]
    public class LocationControllerClass
    {
        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsLocations()
            {
                var controller = new LocationController(
                    id => new ValueTask<IEnumerable<Location>>(new[]
                        {new Location {Latitude = 0, Longitude = 1}}),
                    (id, location) => Task.CompletedTask,
                    new Mock<IMessageHandler<Location, bool>>().Object
                );
                var locations = await controller.GetAsync("00");
                Assert.IsNotNull(locations);
                Assert.IsTrue(locations.Any());
                Assert.IsNotNull(locations.First());
            }
        }

        [TestClass]
        public class PostAsyncMethod
        {
            
        }
    }
}
