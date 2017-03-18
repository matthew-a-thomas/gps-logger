using Common.RemoteStorage.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Command;
using SQLDatabase.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SQLDatabase.Tests.RemoteStorage.Command
{
    [TestClass]
    public class LocationPosterClass
    {
        [TestClass]
        public class PostLocationMethod
        {
            [TestMethod]
            public void AllowsPostingForRandomID()
            {
                var poster = new LocationPoster();
                var id = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(id);
                poster.PostLocation(id, new Location
                {
                    Latitude = 0,
                    Longitude = 0
                });
            }
        }
    }
}
