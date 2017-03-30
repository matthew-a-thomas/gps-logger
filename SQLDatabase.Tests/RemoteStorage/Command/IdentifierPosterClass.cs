using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SQLDatabase.RemoteStorage.Command;

namespace SQLDatabase.Tests.RemoteStorage.Command
{
    [TestClass]
    public class IdentifierPosterClass
    {
        [TestClass]
        public class PostOrGetIdentifierAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopInterfaces()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var identifierPoster = new IdentifierPoster();
                await identifierPoster.PostOrGetIdentifierAsync(mockedTransaction.Object, new byte[0]);
            }

            [TestMethod]
            public async Task HandlesNullParameters()
            {
                var identifierPoster = new IdentifierPoster();
                await identifierPoster.PostOrGetIdentifierAsync(null, null);
            }
        }
    }
}
