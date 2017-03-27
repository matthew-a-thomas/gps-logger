using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.LocalStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.LocalStorage
{
    /// <summary>
    /// Note that executing these tests has side effects: files and directories are created in the temp directory. An effort is made to clean them up, though
    /// </summary>
    [TestClass]
    public class PhysicalStorageClass
    {
        private static DirectoryInfo CreateTempDirectory()
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            directoryInfo.Create();
            return directoryInfo;
        }

        internal static async Task DoWithTempPersistentStoreAsync(Func<PhysicalStorage, Task> actionAsync)
        {
            var directory = CreateTempDirectory();
            var store = new PhysicalStorage(directory, 100);
            try
            {
                await actionAsync(store);
            }
            finally
            {
                directory.Delete(true);
            }
        }

        [TestClass]
        public class ExistsMethod
        {
            [TestMethod]
            public async Task DoesNotBreakWithFunnyNames()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    await store.ExistsAsync(
                        "../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../");
                    await store.ExistsAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|");
                });
            }

            [TestMethod]
            public async Task ReturnsFalseForDummyDirectory()
            {
                var store = new PhysicalStorage(new DirectoryInfo(Guid.NewGuid().ToString()), 100);
                Assert.IsFalse(await store.ExistsAsync(Guid.NewGuid().ToString()));
            }

            [TestMethod]
            public async Task ReturnsTrueForSomethingThatExists()
            {
                var keyName = Guid.NewGuid().ToString();
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    await store.SetAsync(keyName, new byte[0]);
                    Assert.IsTrue(await store.ExistsAsync(keyName));
                });
            }
        }

        [TestClass]
        public class GetAsyncMethod
        {
            [TestMethod]
            public async Task CanGetLongKeyName()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var builder = new StringBuilder();
                    foreach (var piece in Enumerable.Repeat("../", 10000))
                        builder.Append(piece);
                    var key = builder.ToString();
                    await store.GetAsync(key);
                });
            }

            [TestMethod]
            public async Task CanGetFunnyKeyName()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var builder = new StringBuilder();
                    for (var i = 1; i < 256; ++i)
                        builder.Append((char)i);
                    var key = builder.ToString();
                    await store.GetAsync(key);
                });
            }

            [TestMethod]
            public async Task ReturnsNullForNonExistentKey()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var key = Guid.NewGuid().ToString();
                    var result = await store.GetAsync(key);
                    Assert.IsNull(result);
                });
            }

            [TestMethod]
            public async Task ReturnsWhatWasSet()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var key = Guid.NewGuid().ToString();
                    var contents = Guid.NewGuid().ToByteArray();
                    await store.SetAsync(key, contents);
                    var got = await store.GetAsync(key);
                    Assert.IsTrue(contents.SequenceEqual(got));
                });
            }
        }

        [TestClass]
        public class SetAsyncMethod
        {
            [TestMethod]
            public async Task CanSetNewThing()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var key = Guid.NewGuid().ToString();
                    await store.SetAsync(key, new byte[0]);
                });
            }

            [TestMethod]
            public async Task CanSetOldThing()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var key = Guid.NewGuid().ToString();
                    await store.SetAsync(key, new byte[0]);
                    await store.SetAsync(key, new byte[0]);
                });
            }

            [TestMethod]
            public async Task CanSetLongKeyName()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var builder = new StringBuilder();
                    foreach (var piece in Enumerable.Repeat("../", 10000))
                        builder.Append(piece);
                    var key = builder.ToString();
                    await store.SetAsync(
                        key,
                        new byte[0]);
                });
            }

            [TestMethod]
            public async Task CanSetFunnyKeyName()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    var builder = new StringBuilder();
                    for (var i = 1; i < 256; ++i)
                        builder.Append((char) i);
                    var key = builder.ToString();
                    await store.SetAsync(
                        key,
                        new byte[0]);
                });
            }
        }
    }
}
