using System;
using System.Collections.Generic;
using System.Text;
using Common.LocalStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
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
            public void ReturnsFalseForRandomKey()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    var key = Guid.NewGuid().ToString();
                    Assert.IsFalse(manager.IsSet(key), "It said " + key + " existed even though it's completely random");
                });
            }

            [TestMethod]
            public void ReturnsFalseForNull()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    Assert.IsFalse(manager.IsSet(null), "It said null existed even though it's completely random");
                });
            }
        }

        [TestClass]
        public class GetMethod
        {
            [TestMethod]
            public void ReturnsNullForNull()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    Assert.IsNull(manager.Get(null));
                });
            }

            [TestMethod]
            public void ReturnsNullForRandomKey()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    var key = Guid.NewGuid().ToString();
                    Assert.IsNull(manager.Get(key), "It said " + key + " existed even though it's completely random");
                });
            }
        }

        [TestClass]
        public class SetMethod
        {
            [TestMethod]
            public void AcceptsNullKey()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    manager.Set(null, new byte[0]);
                });
            }

            [TestMethod]
            public void AcceptsNullData()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    manager.Set(Guid.NewGuid().ToString(), null);
                });
            }

            [TestMethod]
            public void AcceptsNullKeyAndValue()
            {
                PersistentStoreClass.DoWithTempPersistentStore(store =>
                {
                    var manager = new PersistentStoreManager(store);
                    manager.Set(null, null);
                });
            }
        }

        [TestMethod]
        public void CanGetBackSameDataAsWasSet()
        {
            PersistentStoreClass.DoWithTempPersistentStore(store =>
            {
                var manager = new PersistentStoreManager(store);
                var data = Encoding.ASCII.GetBytes("Hello world!");
                var key = Guid.NewGuid().ToString();
                manager.Set(key, data);
                var retrieved = manager.Get(key);
                Assert.IsNotNull(retrieved);
                Assert.IsTrue(retrieved.SequenceEqual(data));
            });
        }
    }
}
