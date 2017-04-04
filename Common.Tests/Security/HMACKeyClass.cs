using System.Threading.Tasks;
using Common.LocalStorage;
using Common.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Security
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyClass
    {
        private static IStorage<byte[]> CreateStore() => new MemoryStorage<byte[]>();
        private static HMACKey CreateKey() => new HMACKey("", CreateStore(), new KeySizeProvider(16));

        [TestClass]
        public class IsSetAsyncMethod
        {
            [TestMethod]
            public async Task HandlesNopConstructorParameter()
            {
                var mockedStore = new Mock<IStorage<byte[]>>();
                var mockedKeySizeProvider = new Mock<IKeySizeProvider>();
                var controller = new HMACKey("", mockedStore.Object, mockedKeySizeProvider.Object);
                await controller.IsSetAsync();
            }

            [TestMethod]
            public async Task ReturnsFalseAtFirst()
            {
                var controller = CreateKey();
                var get = await controller.IsSetAsync();
                Assert.IsFalse(get);
            }

            [TestMethod]
            public async Task ReturnsTrueWhenSet()
            {
                var controller = CreateKey();
                await controller.SetAsync(new byte[100]);
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
                var mockedKeySizeProvider = new Mock<IKeySizeProvider>();
                var controller = new HMACKey("", mockedStore.Object, mockedKeySizeProvider.Object);
                await controller.SetAsync(new byte[100]);
            }

            [TestMethod]
            public async Task CannotPostEmptyKey()
            {
                var controller = CreateKey();
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
            public async Task CannotPostNull()
            {
                var controller = CreateKey();
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
                var controller = CreateKey();
                await controller.SetAsync(new byte[100]);
                bool failed;
                try
                {
                    await controller.SetAsync(new byte[100]);
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
                var controller = CreateKey();
                bool failed;
                try
                {
                    await controller.SetAsync(new byte[1]);
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
