using Common.RemoteStorage.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Command;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SQLDatabase.Tests.RemoteStorage.Command
{
    [TestClass]
    public class LocationPosterClass
    {
        [TestClass]
        public class PostLocationAsyncMethod
        {
            [TestMethod]
            public async Task AllowsPostingForRandomID()
            {
                await TransactionClass.DoWithTransactionAsync(async transaction =>
                {
                    var identifierPoster = new IdentifierPoster(transaction);
                    var poster = new LocationPoster(identifierPoster, transaction);
                    var id = new byte[16];
                    using (var rng = RandomNumberGenerator.Create())
                        rng.GetBytes(id);
                    await poster.PostLocationAsync(id, new Location
                    {
                        Latitude = 0,
                        Longitude = 0
                    });
                });
            }
        }
    }
}
