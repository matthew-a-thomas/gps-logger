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
            public async Task BlocksMultipleCallsWithSameKey()
            {
                var locker = new Locker<string>();
                var key = Guid.NewGuid().ToString();
                await locker.DoLockedAsync(key, () => Task.Run(() =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(async () =>
                    {
                        startingGate.Set();
                        await locker.DoLockedAsync(key, () => Task.Run(() =>
                        {
                            endingGate.Set();
                        }));
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsFalse(endingGate.IsSet);
                }));
            }

            [TestMethod]
            public async Task DoesNotBlockMultipleCallsWithDifferentKeys()
            {
                var locker = new Locker<string>();
                var key1 = Guid.NewGuid().ToString();
                var key2 = Guid.NewGuid().ToString();
                await locker.DoLockedAsync(key1, () => Task.Run(() =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(async () =>
                    {
                        startingGate.Set();
                        await locker.DoLockedAsync(key2, () => Task.Run(() =>
                        {
                            endingGate.Set();
                        }));
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsTrue(endingGate.IsSet);
                }));
            }

            [TestMethod]
            public async Task DoesNotBlockMultipleCallsWithNullKey()
            {
                var locker = new Locker<string>();
                await locker.DoLockedAsync(null, () => Task.Run(() =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(async () =>
                    {
                        startingGate.Set();
                        await locker.DoLockedAsync(null, () => Task.Run(() =>
                        {
                            endingGate.Set();
                        }));
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsTrue(endingGate.IsSet);
                }));
            }

            [TestMethod]
            public void DoesNothingWithNullAction()
            {
                var locker = new Locker<string>();
                locker.DoLockedAsync("", null);
            }

            [TestMethod]
            public async Task DoesNotBlockWithKeysOfDifferentCase()
            {
                var locker = new Locker<string>();
                var key = Guid.NewGuid().ToString();
                var key1 = key.ToUpper();
                var key2 = key.ToLower();
                Assert.AreNotEqual(key1, key2);
                await locker.DoLockedAsync(key1, () => Task.Run(() =>
                {
                    var startingGate = new ManualResetEventSlim();
                    var endingGate = new ManualResetEventSlim();
                    Task.Run(async () =>
                    {
                        startingGate.Set();
                        await locker.DoLockedAsync(key2, () => Task.Run(() =>
                        {
                            endingGate.Set();
                        }));
                    });
                    startingGate.Wait();
                    endingGate.Wait(TimeSpan.FromMilliseconds(100));
                    Assert.IsTrue(endingGate.IsSet);
                }));
            }
        }
    }
}
