using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SQLDatabase.Tests.RemoteStorage.Query
{
    [TestClass]
    public class LocationProviderClass
    {
        [TestClass]
        public class GetAllLocationsMethod
        {
            [TestMethod]
            public void ReturnsNoLocationsForRandomID()
            {
                var provider = new LocationProvider();
                using (var rng = RandomNumberGenerator.Create())
                {
                    var id = new byte[16];
                    rng.GetBytes(id);
                    var locations = provider.GetAllLocations(id);
                    Assert.IsTrue(!locations.Any());
                }
            }
        }
    }
}
