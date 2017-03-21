using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SQLDatabase.Tests.RemoteStorage.Query
{
    [TestClass]
    public class LocationProviderClass
    {
        [TestClass]
        public class GetAllLocationsAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsNoLocationsForRandomID()
            {
                await TransactionClass.DoWithTransactionAsync(async transaction =>
                {
                    var provider = new LocationProvider(transaction);
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        var id = new byte[16];
                        rng.GetBytes(id);
                        var locations = await provider.GetAllLocationsAsync(id);
                        Assert.IsTrue(!locations.Any());
                    }
                });
            }
        }
    }
}
