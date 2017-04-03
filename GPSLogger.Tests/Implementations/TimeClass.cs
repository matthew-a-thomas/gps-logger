using System;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Messages;
using Common.Security.Signing;
using GPSLogger.Implementations;
using GPSLogger.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Implementations
{
    [TestClass]
    public class TimeClass
    {
        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameters()
            {
                await new Time(new Mock<IMessageHandler<bool, long>>().Object)
                    .GetCurrentTimeAsync(new TimeGetParameters());
            }

            [TestMethod]
            public async Task HandlesNullParameter()
            {
                await new Time(new Mock<IMessageHandler<bool, long>>().Object)
                    .GetCurrentTimeAsync(null);
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
                var controller = new Time(mockedHandler.Object);
                var result = await controller.GetCurrentTimeAsync(null);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Message);
                var difference = Math.Abs(DateTimeOffset.Now.ToUnixTimeSeconds() - result.Message.Contents);
                Assert.IsTrue(difference <= 1, "The returned time was more than a second off");
            }
        }
    }
}
