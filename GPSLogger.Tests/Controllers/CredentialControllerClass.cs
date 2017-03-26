using System;
using System.Threading.Tasks;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using GPSLogger.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Controllers
{
    [TestClass]
    public class CredentialControllerClass
    {
        private static void TestWithNullAndNopConstructorParameters(Action<CredentialController> test)
        {
            test(new CredentialController(
                null,
                null,
                null));
            test(new CredentialController(
                null,
                null,
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialController(
                null,
                id => new ValueTask<Credential<byte[]>>(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                null));
            test(new CredentialController(
                null,
                id => new ValueTask<Credential<byte[]>>(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialController(
                () => new ValueTask<byte[]>(new byte[0]),
                null,
                null));
            test(new CredentialController(
                () => new ValueTask<byte[]>(new byte[0]),
                null,
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialController(
                () => new ValueTask<byte[]>(new byte[0]),
                id => new ValueTask<Credential<byte[]>>(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                null));
            test(new CredentialController(
                () => new ValueTask<byte[]>(new byte[0]),
                id => new ValueTask<Credential<byte[]>>(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
        }

        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void WorksWithNullAndNopParameters()
            {
                TestWithNullAndNopConstructorParameters(_ => { });
            }
        }

        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsEvenFromInvalidRequest()
            {
                var signedMessage = new SignedMessage<Credential<string>>();
                var mockMessageHandler = new Mock<IMessageHandler<bool, Credential<string>>>();
                mockMessageHandler
                    .Setup(handler => handler.CreateResponseAsync(It.IsAny<SignedMessage<bool>>(),
                        It.IsAny<Func<bool, ValueTask<Credential<string>>>>()))
                    .Returns(new ValueTask<SignedMessage<Credential<string>>>(signedMessage));
                var controller = new CredentialController(
                    () => new ValueTask<byte[]>(new byte[0]),
                    id => new ValueTask<Credential<byte[]>>(new Credential<byte[]>
                    {
                        ID = id,
                        Secret = new byte[0]
                    }),
                    mockMessageHandler.Object
                );
                var response = await controller.GetAsync(null);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Contents);
                Assert.AreEqual(signedMessage, response.Contents);
            }

            [TestMethod]
            public void WorksWithNullAndNopParameters()
            {
                TestWithNullAndNopConstructorParameters(controller => controller.GetAsync(null).Wait());
            }
        }
    }
}
