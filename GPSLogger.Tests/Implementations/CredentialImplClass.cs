using System;
using System.Threading.Tasks;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using GPSLogger.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Implementations
{
    [TestClass]
    public class CredentialImplClass
    {
        private static void TestWithNullAndNopConstructorParameters(Action<CredentialImpl> test)
        {
            test(new CredentialImpl(
                null,
                null,
                null));
            test(new CredentialImpl(
                null,
                null,
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialImpl(
                null,
                id => Task.FromResult(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                null));
            test(new CredentialImpl(
                null,
                id => Task.FromResult(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialImpl(
                () => Task.FromResult(new byte[0]),
                null,
                null));
            test(new CredentialImpl(
                () => Task.FromResult(new byte[0]),
                null,
                new Mock<IMessageHandler<bool, Credential<string>>>().Object));
            test(new CredentialImpl(
                () => Task.FromResult(new byte[0]),
                id => Task.FromResult(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
                null));
            test(new CredentialImpl(
                () => Task.FromResult(new byte[0]),
                id => Task.FromResult(new Credential<byte[]> { ID = new byte[0], Secret = new byte[0] }),
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
        public class ProduceFromRequestAsyncMethod
        {
            [TestMethod]
            public async Task ReturnsEvenFromInvalidRequest()
            {
                var mockMessageHandler = new Mock<IMessageHandler<bool, Credential<string>>>();
                mockMessageHandler
                    .Setup(handler => handler.CreateResponseAsync(
                        It.IsAny<SignedMessage<bool>>(),
                        It.IsAny<Func<bool, Task<Credential<string>>>>())
                        )
                    .Returns<SignedMessage<bool>, Func<bool, Task<Credential<string>>>>((request, generateFn) =>
                    {
                        var contentsTask = generateFn(false); // Pretend like it's an invalid request
                        contentsTask.Wait();
                        return Task.FromResult(new SignedMessage<Credential<string>>
                        {
                            Message = new Message<Credential<string>>
                            {
                                Contents = contentsTask.Result
                            }
                        });
                    });
                var controller = new CredentialImpl(
                    () => Task.FromResult(new byte[0]),
                    id => Task.FromResult(new Credential<byte[]>
                    {
                        ID = id,
                        Secret = new byte[0]
                    }),
                    mockMessageHandler.Object
                );
                var response = await controller.ProduceFromRequestAsync(null);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Message.Contents);
            }

            [TestMethod]
            public async Task ReturnsFromValidRequest()
            {
                var mockMessageHandler = new Mock<IMessageHandler<bool, Credential<string>>>();
                mockMessageHandler
                    .Setup(handler => handler.CreateResponseAsync(
                        It.IsAny<SignedMessage<bool>>(),
                        It.IsAny<Func<bool, Task<Credential<string>>>>())
                    )
                    .Returns<SignedMessage<bool>, Func<bool, Task<Credential<string>>>>((request, generateFn) =>
                    {
                        var contentsTask = generateFn(true); // Pretend like it's a valid request
                        contentsTask.Wait();
                        return Task.FromResult(new SignedMessage<Credential<string>>
                        {
                            Message = new Message<Credential<string>>
                            {
                                Contents = contentsTask.Result
                            }
                        });
                    });
                var controller = new CredentialImpl(
                    () => Task.FromResult(new byte[0]),
                    id => Task.FromResult(new Credential<byte[]>
                    {
                        ID = id,
                        Secret = new byte[0]
                    }),
                    mockMessageHandler.Object
                );
                var response = await controller.ProduceFromRequestAsync(null);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Message.Contents);
            }

            [TestMethod]
            public void WorksWithNullAndNopParameters()
            {
                TestWithNullAndNopConstructorParameters(controller => controller.ProduceFromRequestAsync(null).Wait());
            }
        }
    }
}
