using System;
using System.Text;
using Common.LocalStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Tests.LocalStorage
{
    [TestClass]
    public class PersistentStoreManagerClass
    {
        [TestClass]
        public class IsSetMethod
        {
            [TestMethod]
            public async Task ReturnsFalseForRandomKey()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    var key = Guid.NewGuid().ToString();
                    Assert.IsFalse(await manager.IsSetAsync(key), "It said " + key + " existed even though it's completely random");
                });
            }

            [TestMethod]
            public async Task ReturnsFalseForNull()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    Assert.IsFalse(await manager.IsSetAsync(null), "It said null existed even though it's completely random");
                });
            }

            [TestMethod]
            public async Task DoesNotBreakWithFunnyNames()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.IsSetAsync("../../..");
                    await manager.IsSetAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|");
                });
            }
        }

        [TestClass]
        public class GetMethod
        {
            [TestMethod]
            public async Task DoesNotBreakWithFunnyNames()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.GetAsync("../../..");
                    await manager.GetAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|");
                });
            }

            [TestMethod]
            public async Task ReturnsNullForNull()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    Assert.IsNull(await manager.GetAsync(null));
                });
            }

            [TestMethod]
            public async Task ReturnsNullForRandomKey()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    var key = Guid.NewGuid().ToString();
                    Assert.IsNull(await manager.GetAsync(key), "It said " + key + " existed even though it's completely random");
                });
            }
        }

        [TestClass]
        public class SetMethod
        {
            [TestMethod]
            public async Task AcceptsNullKey()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.SetAsync(null, new byte[0]);
                });
            }

            [TestMethod]
            public async Task AcceptsNullData()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.SetAsync(Guid.NewGuid().ToString(), null);
                });
            }

            [TestMethod]
            public async Task AcceptsNullKeyAndValue()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.SetAsync(null, null);
                });
            }

            [TestMethod]
            public async Task DoesNotBreakWithFunnyNames()
            {
                await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
                {
                    var manager = new PersistentStoreManager(store);
                    await manager.SetAsync("../../..", new byte[0]);
                    await manager.SetAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|", new byte[0]);
                });
            }
        }

        [TestMethod]
        public async Task CanGetBackSameDataAsWasSet()
        {
            await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
            {
                var manager = new PersistentStoreManager(store);
                var data = Encoding.ASCII.GetBytes("Hello world!");
                var key = Guid.NewGuid().ToString();
                await manager.SetAsync(key, data);
                var retrieved = await manager.GetAsync(key);
                Assert.IsNotNull(retrieved);
                Assert.IsTrue(retrieved.SequenceEqual(data));
            });
        }

        [TestMethod]
        public async Task CanGetBackSameDataAsWasSetWithDifferentCasedKey()
        {
            await PersistentStoreClass.DoWithTempPersistentStoreAsync(async store =>
            {
                var manager = new PersistentStoreManager(store);
                var data = Encoding.ASCII.GetBytes("Hello world!");
                var key = Guid.NewGuid().ToString();
                var uppercased = key.ToUpper();
                var lowercased = key.ToLower();
                Assert.AreNotEqual(uppercased, lowercased);
                await manager.SetAsync(uppercased, data);
                var retrieved = await manager.GetAsync(lowercased);
                Assert.IsNotNull(retrieved);
                Assert.IsTrue(retrieved.SequenceEqual(data));
            });
        }
    }
}
