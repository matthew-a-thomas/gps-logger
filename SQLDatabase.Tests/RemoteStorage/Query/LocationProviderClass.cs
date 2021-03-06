﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;
using SQLDatabase.RemoteStorage.Query;

namespace SQLDatabase.Tests.RemoteStorage.Query
{
    [TestClass]
    public class LocationProviderClass
    {
        [TestClass]
        public class GetAllLocationsAsyncMethod
        {
            [TestMethod]
            public async Task DoesNotCommitTransaction()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var provider = new LocationProvider(() => mockedTransaction.Object);
                await provider.GetAllLocationsAsync(new byte[0]);
                mockedTransaction.Verify(transaction => transaction.Commit(), Times.Never);
            }

            [TestMethod]
            public async Task HandlesNopInterfaces()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var provider = new LocationProvider(() => mockedTransaction.Object);
                await provider.GetAllLocationsAsync(new byte[0]);
            }

            [TestMethod]
            public async Task HandlesNullParameters()
            {
                var provider = new LocationProvider(() => null);
                await provider.GetAllLocationsAsync(null);

                var provider2 = new LocationProvider(null);
                await provider2.GetAllLocationsAsync(null);
            }

            [TestMethod]
            public async Task ReturnsCorrectResults()
            {
                var mockedTransaction = new Mock<ITransaction>();
                mockedTransaction
                    .Setup(
                        transaction =>
                        transaction.GetResultsAsync(
                            It.IsAny<Commands.Command>()
                        )
                    )
                    .Returns(
                        Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, object>>>(
                            new[]
                            {
                                new Dictionary<string, object>
                                {
                                    { "latitude", -9 },
                                    { "longitude", 10 },
                                    { "unixTime", 1000 }
                                },
                                new Dictionary<string, object>
                                {
                                    { "latitude", -8 },
                                    { "longitude", 11 },
                                    { "unixTime", 2000 }
                                }
                            }
                        )
                    );
                var provider = new LocationProvider(() => mockedTransaction.Object);
                var locations = await provider.GetAllLocationsAsync(new byte[0]);
                var list = locations.ToArray();
                Assert.AreEqual(2, list.Length);

                Assert.AreEqual(-9, list[0].Latitude);
                Assert.AreEqual(10, list[0].Longitude);
                Assert.AreEqual(1000, list[0].UnixTime);

                Assert.AreEqual(-8, list[1].Latitude);
                Assert.AreEqual(11, list[1].Longitude);
                Assert.AreEqual(2000, list[1].UnixTime);
            }
        }
    }
}
