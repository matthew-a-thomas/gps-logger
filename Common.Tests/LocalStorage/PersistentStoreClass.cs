using System;
using System.IO;
using System.Threading.Tasks;
using Common.LocalStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.LocalStorage
{
    /// <summary>
    /// Note that executing these tests has side effects: files and directories are created in the temp directory. An effort is made to clean them up, though
    /// </summary>
    [TestClass]
    public class PersistentStoreClass
    {
        private static DirectoryInfo CreateTempDirectory()
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            directoryInfo.Create();
            return directoryInfo;
        }

        internal static async Task DoWithTempPersistentStoreAsync(Func<PersistentStore, Task> actionAsync)
        {
            var directory = CreateTempDirectory();
            var store = new PersistentStore(directory);
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
                    await store.ExistsAsync("../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../");
                    await store.ExistsAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|");
                });
            }

            [TestMethod]
            public async Task ReturnsFalseForDummyDirectory()
            {
                var store = new PersistentStore(new DirectoryInfo(Guid.NewGuid().ToString()));
                Assert.IsFalse(await store.ExistsAsync(Guid.NewGuid().ToString()));
            }

            [TestMethod]
            public async Task ReturnsTrueForSomethingThatExists()
            {
                var keyName = Guid.NewGuid().ToString();
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (await store.OpenAsync(keyName, new Options
                    {
                        FileAccess = FileAccess.ReadWrite,
                        FileMode = FileMode.OpenOrCreate,
                        FileShare = FileShare.None
                    }))
                    {
                        Assert.IsTrue(await store.ExistsAsync(keyName));
                    }
                });
            }
        }

        [TestClass]
        public class OpenMethod
        {
            [TestMethod]
            public async Task CanCreateRandomKey()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (await store.OpenAsync(Guid.NewGuid().ToString(), new Options
                    {
                        FileAccess = FileAccess.ReadWrite,
                        FileMode = FileMode.OpenOrCreate,
                        FileShare = FileShare.None
                    }))
                    {

                    }
                });
            }

            [TestMethod]
            public async Task CanWriteToStreamOpenedAsWrite()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (var stream = await store.OpenAsync(Guid.NewGuid().ToString(), new Options
                    {
                        FileAccess = FileAccess.Write,
                        FileMode = FileMode.CreateNew,
                        FileShare = FileShare.None
                    }))
                    {
                        stream.WriteByte(0xFF);
                    }
                });
            }

            [TestMethod]
            public async Task CannotOpenNonSharedKeyTwice()
            {
                var failed = false;
                var keyName = Guid.NewGuid().ToString();
                var options = new Options
                {
                    FileAccess = FileAccess.ReadWrite,
                    FileMode = FileMode.OpenOrCreate,
                    FileShare = FileShare.None
                };
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (await store.OpenAsync(keyName, options))
                    {
                        try
                        {
                            using (await store.OpenAsync(keyName, options))
                            { }
                        }
                        catch
                        {
                            failed = true;
                        }
                    }
                });
                Assert.IsTrue(failed);
            }

            [TestMethod]
            public async Task CannotWriteToStreamOpenedAsRead()
            {
                var failed = false;
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (var stream = await store.OpenAsync(Guid.NewGuid().ToString(), new Options
                    {
                        FileAccess = FileAccess.Read,
                        FileMode = FileMode.OpenOrCreate,
                        FileShare = FileShare.None
                    }))
                    {
                        try
                        {
                            stream.WriteByte(0xFF);
                        }
                        catch
                        {
                            failed = true;
                        }
                    }
                });
                Assert.IsTrue(failed);
            }

            [TestMethod]
            public async Task DoesNotBreakWithFunnyNames()
            {
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    using (
                        await store.OpenAsync(
                            "../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../",
                            new Options
                            {
                                FileAccess = FileAccess.ReadWrite,
                                FileMode = FileMode.OpenOrCreate,
                                FileShare = FileShare.ReadWrite
                            }))
                    {
                    }
                    using (
                        await store.OpenAsync("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|",
                            new Options
                            {
                                FileAccess = FileAccess.ReadWrite,
                                FileMode = FileMode.OpenOrCreate,
                                FileShare = FileShare.ReadWrite
                            }))
                    {
                    }
                });
            }

            [TestMethod]
            public async Task FailsToOpenNewKey()
            {
                var failed = false;
                await DoWithTempPersistentStoreAsync(async store =>
                {
                    try
                    {
                        using (await store.OpenAsync(Guid.NewGuid().ToString(), new Options
                        {
                            FileAccess = FileAccess.Read,
                            FileMode = FileMode.Open,
                            FileShare = FileShare.None
                        }))
                        {

                        }
                    }
                    catch
                    {
                        failed = true;
                    }
                });
                Assert.IsTrue(failed);
            }
        }
    }
}
