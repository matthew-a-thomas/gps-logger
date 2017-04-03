using System.Threading.Tasks;
using Common.Extensions;
using Common.LocalStorage;
using GPSLogger.Implementations;
using GPSLogger.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GPSLogger.Tests.Implementations
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyClass
    {
        private static IStorage<byte[]> CreateStore() => new MemoryStorage<byte[]>();

        [TestClass]
        public class IsSetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage<byte[]>>();
                var controller = new HMACKey(mockedStore.Object);
                await controller.IsSetAsync();
            }

            [TestMethod]
            public async Task ReturnsFalseAtFirst()
            {
                var store = CreateStore();
                var controller = new HMACKey(store);
                var get = await controller.IsSetAsync();
                Assert.IsFalse(get);
            }

            [TestMethod]
            public async Task ReturnsTrueWhenSet()
            {
                var store = CreateStore();
                var controller = new HMACKey(store);
                await controller.SetAsync(new HMACPostParameters
                {
                    NewKey = new byte[100].ToHexString()
                });
                var get = await controller.IsSetAsync();
                Assert.IsTrue(get);
            }
        }

        [TestClass]
        public class SetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage<byte[]>>();
                var controller = new HMACKey(mockedStore.Object);
                await controller.SetAsync(new HMACPostParameters { NewKey = new byte[100].ToHexString() });
            }

            [TestMethod]
            public async Task CannotPostEmptyKey()
            {
                var controller = new HMACKey(CreateStore());
                bool failed;
                try
                {
                    await controller.SetAsync(new HMACPostParameters());
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
                var controller = new HMACKey(CreateStore());
                bool failed;
                try
                {
                    await controller.SetAsync(null);
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
                var controller = new HMACKey(CreateStore());
                await controller.SetAsync(new HMACPostParameters { NewKey = new byte[100].ToHexString() });
                bool failed;
                try
                {
                    await controller.SetAsync(
                        new HMACPostParameters {NewKey = new byte[100].ToHexString()});
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
                var controller = new HMACKey(CreateStore());
                bool failed;
                try
                {
                    await controller.SetAsync(
                        new HMACPostParameters { NewKey = new byte[1].ToHexString() });
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
