using System.Threading.Tasks;
using Common.Extensions;
using Common.LocalStorage;
using GPSLogger.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Controllers
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyControllerClass
    {
        private static IStorage<byte[]> CreateStore() => new MemoryStorage<byte[]>();

        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage<byte[]>>();
                var controller = new HMACKeyController(mockedStore.Object);
                await controller.GetAsync();
            }

            [TestMethod]
            public async Task ReturnsFalseAtFirst()
            {
                var store = CreateStore();
                var controller = new HMACKeyController(store);
                var get = await controller.GetAsync();
                Assert.IsFalse(get);
            }

            [TestMethod]
            public async Task ReturnsTrueWhenSet()
            {
                var store = CreateStore();
                var controller = new HMACKeyController(store);
                await controller.PostAsync(new HMACKeyController.PostParameters
                {
                    NewKey = new byte[100].ToHexString()
                });
                var get = await controller.GetAsync();
                Assert.IsTrue(get);
            }
        }

        [TestClass]
        public class PostAsyncClass
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage<byte[]>>();
                var controller = new HMACKeyController(mockedStore.Object);
                await controller.PostAsync(new HMACKeyController.PostParameters { NewKey = new byte[100].ToHexString() });
            }

            [TestMethod]
            public async Task CannotPostEmptyKey()
            {
                var controller = new HMACKeyController(CreateStore());
                bool failed;
                try
                {
                    await controller.PostAsync(new HMACKeyController.PostParameters());
                    failed = false;
                }
                catch
                {
                    failed = true;
                }
                Assert.IsTrue(failed);
            }

            [TestMethod]
            public async Task CannotPostNull()
            {
                var controller = new HMACKeyController(CreateStore());
                bool failed;
                try
                {
                    await controller.PostAsync(null);
                    failed = false;
                }
                catch
                {
                    failed = true;
                }
                Assert.IsTrue(failed);
            }

            [TestMethod]
            public async Task CannotPostTwice()
            {
                var controller = new HMACKeyController(CreateStore());
                await controller.PostAsync(new HMACKeyController.PostParameters { NewKey = new byte[100].ToHexString() });
                bool failed;
                try
                {
                    await controller.PostAsync(
                        new HMACKeyController.PostParameters {NewKey = new byte[100].ToHexString()});
                    failed = false;
                }
                catch
                {
                    failed = true;
                }
                Assert.IsTrue(failed);
            }

            [TestMethod]
            public async Task CannotPostSmallKey()
            {
                var controller = new HMACKeyController(CreateStore());
                bool failed;
                try
                {
                    await controller.PostAsync(
                        new HMACKeyController.PostParameters { NewKey = new byte[1].ToHexString() });
                    failed = false;
                }
                catch
                {
                    failed = true;
                }
                Assert.IsTrue(failed);
            }
        }
    }
}
