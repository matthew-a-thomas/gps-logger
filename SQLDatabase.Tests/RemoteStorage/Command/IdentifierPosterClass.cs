using System.Collections.Generic;
using System.Linq;
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
            public async Task InvokesGetScalarAsync()
            {
                var mockedTransaction = new Mock<ITransaction>();
                var poster = new IdentifierPoster();
                await poster.PostOrGetIdentifierAsync(mockedTransaction.Object, new byte[0]);
                mockedTransaction.Verify(transaction => transaction.GetScalarAsync(It.IsAny<Commands.Command>()));
            }

            [TestMethod]
            public async Task PassesAlongIdentifier()
            {
                var mockedTransaction = new Mock<ITransaction>();
                IDictionary<string, object> parameters = null;
                mockedTransaction.Setup(transaction => transaction.GetScalarAsync(It.IsAny<Commands.Command>()))
                    .Returns<Commands.Command>(
                        command =>
                        {
                            parameters = command.Parameters;
                            return new ValueTask<object>(99);
                        });
                var poster = new IdentifierPoster();
                await poster.PostOrGetIdentifierAsync(mockedTransaction.Object, new byte[0]);
                mockedTransaction.Verify(transaction => transaction.GetScalarAsync(It.IsAny<Commands.Command>()));
                Assert.IsNotNull(parameters);
                Assert.AreEqual(1, parameters.Keys.Count);
                Assert.IsTrue(parameters.First().Value is byte[]);
                Assert.AreEqual(0, (parameters.First().Value as byte[])?.Length);
            }
        }
    }
}
