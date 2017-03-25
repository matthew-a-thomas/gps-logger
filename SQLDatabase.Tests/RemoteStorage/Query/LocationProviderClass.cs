using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SQLDatabase.Commands;
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
            public async Task RequestsOneTransaction()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var invokationCount = 0;
                var provider = new LocationProvider(() =>
                {
                    Interlocked.Increment(ref invokationCount);
                    return mockedTransaction.Object;
                });
                await provider.GetAllLocationsAsync(new byte[0]);
                Assert.AreEqual(1, invokationCount);
            }

            [TestMethod]
            public async Task ReturnsCorrectResults()
            {
                var mockedTransaction = new Mock<ITransaction>();
                mockedTransaction
                    .Setup(
                        transaction =>
                        transaction.GetResultsAsync(
                            It.IsAny<Command>()
                        )
                    )
                    .Returns(
                        new ValueTask<IEnumerable<IReadOnlyDictionary<string, object>>>(
                            new[]
                            {
                                new Dictionary<string, object>
                                {
                                    { "latitude", -9 },
                                    { "longitude", 10 },
                                    { "timestamp", new DateTime(1000) }
                                },
                                new Dictionary<string, object>
                                {
                                    { "latitude", -8 },
                                    { "longitude", 11 },
                                    { "timestamp", new DateTime(2000) }
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
                Assert.AreEqual(1000, list[0].Timestamp.Ticks);

                Assert.AreEqual(-8, list[1].Latitude);
                Assert.AreEqual(11, list[1].Longitude);
                Assert.AreEqual(2000, list[1].Timestamp.Ticks);
            }

            [TestMethod]
            public async Task DoesNotCommitTransaction()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var provider = new LocationProvider(() => mockedTransaction.Object);
                await provider.GetAllLocationsAsync(new byte[0]);
                mockedTransaction.Verify(transaction => transaction.Commit(), Times.Never);
            }
        }
    }
}
