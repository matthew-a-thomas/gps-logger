using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Utilities
{
    [TestClass]
    public class EphemeronClass
    {
        [TestMethod]
        public void InvokesCallbackOnFinalization()
        {
            var invoked = false;
            Task.Run(() => new Ephemeron(() => invoked = true)).Wait();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public void DoesNotInvokeCallbackWhenNotFinalized()
        {
            var invoked = false;
            var ephemeron = new Ephemeron(() => invoked = true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(invoked);
        }
    }
}
