using System;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Messages;
using Common.Security.Signing;
using GPSLogger.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Controllers
{
    [TestClass]
    public class TimeControllerClass
    {
        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameters()
            {
                await new TimeController(new Mock<IMessageHandler<bool, long>>().Object)
                    .GetAsync(new TimeController.GetParameters());
            }

            [TestMethod]
            public async Task HandlesNullParameter()
            {
                await new TimeController(new Mock<IMessageHandler<bool, long>>().Object)
                    .GetAsync(null);
            }

            [TestMethod]
            public async Task ReturnsTime()
            {
                var mockedHandler = new Mock<IMessageHandler<bool, long>>();
                mockedHandler
                    .Setup(handler => handler.CreateResponseAsync(It.IsAny<SignedMessage<bool>>(),
                        It.IsAny<Func<bool, Task<long>>>()))
                    .Returns<SignedMessage<bool>, Func<bool, Task<long>>>((request, generator) =>
                    {
                        var time = generator(true).WaitAndGet();
                        return Task.FromResult(new SignedMessage<long>
                        {
                            Message = new Message<long>
                            {
                                Contents = time
                            }
                        });
                    });
                var controller = new TimeController(mockedHandler.Object);
                var result = await controller.GetAsync(null);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Message);
                var difference = Math.Abs(DateTimeOffset.Now.ToUnixTimeSeconds() - result.Message.Contents);
                Assert.IsTrue(difference <= 1, "The returned time was more than a second off");
            }
        }
    }
}
