using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLDatabase.RemoteStorage.Command;
using System;
using System.Collections.Generic;
using System.Text;
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
                    var poster = new IdentifierPoster();
                    var identifier = Guid.NewGuid().ToByteArray();
                    var id = await poster.PostOrGetIdentifierAsync(transaction, identifier);
                });
            }
        }
    }
}
