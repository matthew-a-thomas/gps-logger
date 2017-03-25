using System;
using System.Threading;
using Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class ActionExtensionsClass
    {
        [TestClass]
        public class MakeSingularMethod
        {
            [TestMethod]
            public void HandlesNull()
            {
                ActionExtensions.MakeSingular(null);
            }

            [TestClass]
            public class Result
            {
                [TestMethod]
                public void CanBeInvoked()
                {
                    var invoked = false;
                    var action = new Action(() => invoked = true);
                    var result = action.MakeSingular();
                    result();
                    Assert.IsTrue(invoked);
                }

                [TestMethod]
                public void CannotBeInvokedTwice()
                {
                    var invocationCount = 0;
                    var action = new Action(() => Interlocked.Increment(ref invocationCount));
                    var result = action.MakeSingular();
                    result();
                    result();
                    Assert.AreEqual(1, invocationCount);
                }
            }
        }
    }
}
