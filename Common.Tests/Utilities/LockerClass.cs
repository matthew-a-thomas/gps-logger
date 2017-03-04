using System;
using System.Collections.Generic;
using System.Text;
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
                var locker = new Locker<string>();
                var key = Guid.NewGuid().ToString();
                locker.DoLocked(key, () =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(() =>
                    {
                        startingGate.Set();
                        locker.DoLocked(key, () =>
                        {
                            endingGate.Set();
                        });
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsFalse(endingGate.IsSet);
                });
            }

            [TestMethod]
            public void DoesNotBlockMultipleCallsWithDifferentKeys()
            {
                var locker = new Locker<string>();
                var key1 = Guid.NewGuid().ToString();
                var key2 = Guid.NewGuid().ToString();
                locker.DoLocked(key1, () =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(() =>
                    {
                        startingGate.Set();
                        locker.DoLocked(key2, () =>
                        {
                            endingGate.Set();
                        });
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsTrue(endingGate.IsSet);
                });
            }

            [TestMethod]
            public void DoesNotBlockMultipleCallsWithNullKey()
            {
                var locker = new Locker<string>();
                locker.DoLocked(null, () =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(() =>
                    {
                        startingGate.Set();
                        locker.DoLocked(null, () =>
                        {
                            endingGate.Set();
                        });
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsTrue(endingGate.IsSet);
                });
            }

            [TestMethod]
            public void DoesNothingWithNullAction()
            {
                var locker = new Locker<string>();
                locker.DoLocked("", null);
            }
        }
    }
}
