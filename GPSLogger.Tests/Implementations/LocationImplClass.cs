using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Messages;
using GPSLogger.Implementations;
using GPSLogger.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Implementations
{
    [TestClass]
    public class LocationImplClass
    {
        [TestClass]
        public class GetLocationsForAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsLocations()
            {
                var controller = new LocationImpl(
                    id => Task.FromResult<IEnumerable<Common.RemoteStorage.Models.Location>>(new[]
                        {new Common.RemoteStorage.Models.Location {Latitude = 0, Longitude = 1, UnixTime = 99}}),
                    (id, location) => Task.CompletedTask,
                    new Mock<IMessageHandler<Location, bool>>().Object
                );
                var locations = (await controller.GetLocationsForAsync("00"))?.ToList();
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
