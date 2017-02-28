using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPS_Logger.LocalStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPS_Logger.Tests.LocalStorage
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

        private static void DoWithTempDirectory(Action<PersistentStore> action)
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
            public void ReturnsFalseForDummyDirectory()
            {
                var store = new PersistentStore(new DirectoryInfo(Guid.NewGuid().ToString()));
                Assert.IsFalse(store.Exists(Guid.NewGuid().ToString()));
            }

            [TestMethod]
            public void ReturnsTrueForSomethingThatExists()
            {
                var keyName = Guid.NewGuid().ToString();
                DoWithTempDirectory(store =>
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
                DoWithTempDirectory(store =>
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
                DoWithTempDirectory(store =>
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
                DoWithTempDirectory(store =>
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
                DoWithTempDirectory(store =>
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
            public void FailsToOpenNewKey()
            {
                var failed = false;
                DoWithTempDirectory(store =>
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
