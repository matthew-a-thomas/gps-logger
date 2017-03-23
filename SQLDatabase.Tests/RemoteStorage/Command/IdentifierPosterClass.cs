using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Command;
using System;
using System.Threading.Tasks;

namespace SQLDatabase.Tests.RemoteStorage.Command
{
    [TestClass]
    public class IdentifierPosterClass
    {
        [TestClass]
        public class PostOrGetIdentifierAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsSomething()
            {
                await TransactionClass.DoWithTransactionAsync(async transaction =>
                {
                    var poster = new IdentifierPoster(transaction);
                    var identifier = Guid.NewGuid().ToByteArray();
                    await poster.PostOrGetIdentifierAsync(identifier);
                });
            }

            [TestMethod]
            // ReSharper disable once InconsistentNaming
            public async Task ReturnsTheIDThatWasJustCreated()
            {
                await TransactionClass.DoWithTransactionAsync(async transaction =>
                {
                    var poster = new IdentifierPoster(transaction);
                    var identifier = Guid.NewGuid().ToByteArray();
                    var id = await poster.PostOrGetIdentifierAsync(identifier);
                    // ReSharper disable once InconsistentNaming
                    var secondID = await poster.PostOrGetIdentifierAsync(identifier);
                    Assert.AreEqual(id, secondID);
                });
            }
        }
    }
}
