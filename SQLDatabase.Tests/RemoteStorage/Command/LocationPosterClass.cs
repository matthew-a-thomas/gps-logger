using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.RemoteStorage.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SQLDatabase.RemoteStorage.Command;

namespace SQLDatabase.Tests.RemoteStorage.Command
{
    [TestClass]
    public class LocationPosterClass
    {
        [TestClass]
        public class PostLocationsAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopInterfaces()
            {
                var mockedIdentifierPoster = new Mock<IIdentifierPoster>();
                var mockedTransaction = new Mock<ITransaction>();
                var locationPoster = new LocationPoster(mockedIdentifierPoster.Object, () => mockedTransaction.Object);
                await locationPoster.PostLocationAsync(new byte[0], new Location {Latitude = 11, Longitude = 12});
            }

            [TestMethod]
            public async Task HandlesNullParameters()
            {
                var mockedIdentifierPoster = new Mock<IIdentifierPoster>();
                var mockedTransaction = new Mock<ITransaction>();
                var locationPoster = new LocationPoster(mockedIdentifierPoster.Object, () => mockedTransaction.Object);
                await locationPoster.PostLocationAsync(null, null);
            }
        }
    }
}
