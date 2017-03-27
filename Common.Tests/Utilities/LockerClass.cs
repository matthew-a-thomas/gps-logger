using System;
using System.Threading;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Common.Tests.Utilities
{
    [TestClass]
    public class LockerClass
    {
        [TestClass]
        public class DoLockedMethod
        {
            [TestMethod]
            public void BlocksMultipleCallsWithSameKey()
            {
                throw new NotImplementedException();
                //var locker = new Locker<string>();
                //var key = Guid.NewGuid().ToString();
                //await locker.DoLocked(key, () => Task.Run(() =>
                //{
                //    var startingGate = new ManualResetEventSlim();
                //    var endingGate = new ManualResetEventSlim();
                //    Task.Run(async () =>
                //    {
                //        startingGate.Set();
                //        await locker.DoLocked(key, () => Task.Run(() =>
                //        {
                //            endingGate.Set();
                //        }));
                //    });
                //    startingGate.Wait();
                //    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                //    Assert.IsFalse(endingGate.IsSet);
                //}));
            }

            [TestMethod]
            public void DoesNotBlockMultipleCallsWithDifferentKeys()
            {
                throw new NotImplementedException();
                //var locker = new Locker<string>();
                //var key1 = Guid.NewGuid().ToString();
                //var key2 = Guid.NewGuid().ToString();
                //await locker.DoLocked(key1, () => Task.Run(() =>
                //{
                //    var startingGate = new ManualResetEventSlim();
                //    var endingGate = new ManualResetEventSlim();
                //    Task.Run(async () =>
                //    {
                //        startingGate.Set();
                //        await locker.DoLocked(key2, () => Task.Run(() =>
                //        {
                //            endingGate.Set();
                //        }));
                //    });
                //    startingGate.Wait();
                //    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                //    Assert.IsTrue(endingGate.IsSet);
                //}));
            }

            [TestMethod]
            public void DoesNotBlockMultipleCallsWithNullKey()
            {
                throw new NotImplementedException();
                //var locker = new Locker<string>();
                //await locker.DoLocked(null, () => Task.Run(() =>
                //{
                //    var startingGate = new ManualResetEventSlim();
                //    var endingGate = new ManualResetEventSlim();
                //    Task.Run(async () =>
                //    {
                //        startingGate.Set();
                //        await locker.DoLocked(null, () => Task.Run(() =>
                //        {
                //            endingGate.Set();
                //        }));
                //    });
                //    startingGate.Wait();
                //    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                //    Assert.IsTrue(endingGate.IsSet);
                //}));
            }

            [TestMethod]
            public void DoesNothingWithNullAction()
            {
                throw new NotImplementedException();
                //var locker = new Locker<string>();
                //await locker.DoLocked("", null);
            }

            [TestMethod]
            public void DoesNotBlockWithKeysOfDifferentCase()
            {
                throw new NotImplementedException();
                //var locker = new Locker<string>();
                //var key = Guid.NewGuid().ToString();
                //var key1 = key.ToUpper();
                //var key2 = key.ToLower();
                //Assert.AreNotEqual(key1, key2);
                //await locker.DoLocked(key1, () => Task.Run(() =>
                //{
                //    var startingGate = new ManualResetEventSlim();
                //    var endingGate = new ManualResetEventSlim();
                //    Task.Run(async () =>
                //    {
                //        startingGate.Set();
                //        await locker.DoLocked(key2, () => Task.Run(() =>
                //        {
                //            endingGate.Set();
                //        }));
                //    });
                //    startingGate.Wait();
                //    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                //    Assert.IsTrue(endingGate.IsSet);
                //}));
            }
        }
    }
}
