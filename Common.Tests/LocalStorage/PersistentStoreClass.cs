using System;
using System.IO;
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

        internal static void DoWithTempPersistentStore(Action<PersistentStore> action)
        {
            var directory = CreateTempDirectory();
            var store = new PersistentStore(directory);
            try
            {
                action(store);
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
            public void DoesNotBreakWithFunnyNames()
            {
                DoWithTempPersistentStore(store =>
                {
                    store.Exists("../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../");
                    store.Exists("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|");
                });
            }

            [TestMethod]
            public void ReturnsFalseForDummyDirectory()
            {
                var store = new PersistentStore(new DirectoryInfo(Guid.NewGuid().ToString()));
                Assert.IsFalse(store.Exists(Guid.NewGuid().ToString()));
            }

            [TestMethod]
            public void ReturnsTrueForSomethingThatExists()
            {
                var keyName = Guid.NewGuid().ToString();
                DoWithTempPersistentStore(store =>
                {
                    using (store.Open(keyName, new Options
                    {
                        FileAccess = FileAccess.ReadWrite,
                        FileMode = FileMode.OpenOrCreate,
                        FileShare = FileShare.None
                    }))
                    {
                        Assert.IsTrue(store.Exists(keyName));
                    }
                });
            }
        }

        [TestClass]
        public class OpenMethod
        {
            [TestMethod]
            public void CanCreateRandomKey()
            {
                DoWithTempPersistentStore(store =>
                {
                    using (store.Open(Guid.NewGuid().ToString(), new Options
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
            public void CanWriteToStreamOpenedAsWrite()
            {
                DoWithTempPersistentStore(store =>
                {
                    using (var stream = store.Open(Guid.NewGuid().ToString(), new Options
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
            public void CannotOpenNonSharedKeyTwice()
            {
                var failed = false;
                var keyName = Guid.NewGuid().ToString();
                var options = new Options
                {
                    FileAccess = FileAccess.ReadWrite,
                    FileMode = FileMode.OpenOrCreate,
                    FileShare = FileShare.None
                };
                DoWithTempPersistentStore(store =>
                {
                    using (store.Open(keyName, options))
                    {
                        try
                        {
                            using (store.Open(keyName, options))
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
            public void CannotWriteToStreamOpenedAsRead()
            {
                var failed = false;
                DoWithTempPersistentStore(store =>
                {
                    using (var stream = store.Open(Guid.NewGuid().ToString(), new Options
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
            public void DoesNotBreakWithFunnyNames()
            {
                DoWithTempPersistentStore(store =>
                {
                    using (
                        store.Open(
                            "../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../../",
                            new Options
                            {
                                FileAccess = FileAccess.ReadWrite,
                                FileMode = FileMode.OpenOrCreate,
                                FileShare = FileShare.ReadWrite
                            }))
                    {
                    }
                    using (
                        store.Open("a/b/c/d/!@#$%^&*()_+~`[]{};':\",./<>?\\|",
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
            public void FailsToOpenNewKey()
            {
                var failed = false;
                DoWithTempPersistentStore(store =>
                {
                    try
                    {
                        using (store.Open(Guid.NewGuid().ToString(), new Options
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
