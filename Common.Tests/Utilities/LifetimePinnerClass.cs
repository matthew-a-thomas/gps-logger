using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Utilities
{
    [TestClass]
    public class LifetimePinnerClass
    {
        [TestClass]
        public class PinMethod
        {
            [TestMethod]
            public void DoesNotFinalizeThingWhenOtherThingIsNotFinalized()
            {
                var pinner = new LifetimePinner<object, object>();
                WeakReference<object> reference = null;
                var gate = new ManualResetEventSlim();
                var to = new object();
                Task.Run(() =>
                {
                    var thing = new object();
                    reference = new WeakReference<object>(thing);
                    pinner.Pin(thing, to);
                    gate.Set();
                });
                gate.Wait();

                GC.Collect(2);
                GC.WaitForPendingFinalizers();

                object o;
                Assert.IsTrue(reference.TryGetTarget(out o), "Thing was finalized even though it was supposed to have been pinned to something we still have a strong reference to");
            }

            [TestMethod]
            public void DoesFinalizeThingWhenOtherThingIsFinalized()
            {
                var pinner = new LifetimePinner<object, object>();
                WeakReference<object> reference = null;
                var gate = new ManualResetEventSlim();
                Task.Run(() =>
                {
                    var thing = new object();
                    reference = new WeakReference<object>(thing);
                    var to = new object();
                    pinner.Pin(thing, to);
                    gate.Set();
                });
                gate.Wait();

                for (var i = 0; i < 2; ++i)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                object o;
                Assert.IsFalse(reference.TryGetTarget(out o), "Thing was not finalized even though the thing it was pinned to was finalized");
            }

            [TestMethod]
            public void DoesNotFinalizeThingWhenOtherThingIsFinalizedIfItIsPinnedToSomethingElse()
            {
                var pinner = new LifetimePinner<object, object>();
                WeakReference<object> reference = null;
                var gate = new ManualResetEventSlim();
                var permanentTo = new object();
                Task.Run(() =>
                {
                    var thing = new object();
                    reference = new WeakReference<object>(thing);
                    var to = new object();
                    pinner.Pin(thing, to);
                    pinner.Pin(thing, permanentTo);
                    gate.Set();
                });
                gate.Wait();

                for (var i = 0; i < 10; ++i)
                {
                    GC.Collect(2);
                    GC.WaitForPendingFinalizers();
                }

                object o;
                Assert.IsTrue(reference.TryGetTarget(out o), "Thing was finalized even though one of the things it was pinned to was not finalized");
            }

            [TestMethod]
            public void WorksWithAutoFinalizerSoThatSomethingCanBeDisposedWhenAnyOfTheThingsItIsPinnedToIsFinalized()
            {
                var pinner = new LifetimePinner<AutoFinalizer, object>();
#pragma warning disable 219
                WeakReference<object> reference = null;
#pragma warning restore 219
                var gate = new ManualResetEventSlim();
                var permanentTo = new object();
                var finalized = false;
                Task.Run(() =>
                {
                    var to = new object();
                    pinner.Pin(new AutoFinalizer(() => finalized = true), to);
                    pinner.Pin(new AutoFinalizer(() => finalized = true), permanentTo);
                    gate.Set();
                });
                gate.Wait();

                for (var i = 0; i < 10; ++i)
                {
                    GC.Collect(2);
                    GC.WaitForPendingFinalizers();
                }

                Assert.IsTrue(finalized, "Didn't finalize, even though one of the things it was pinned to finalized");
            }
        }
    }
}
