using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPS_Logger.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPS_Logger.Tests.Utilities
{
    [TestClass]
    public class ReplayDetectorClass
    {
        [TestClass]
        public class IsNewMethod
        {
            [TestMethod]
            public void RecognizesNewThingAsNew()
            {
                var detector = new ReplayDetector<string>(TimeSpan.FromSeconds(1));
                Assert.IsTrue(detector.IsNew("new thing"));
            }

            [TestMethod]
            public void SaysOldThingIsOld()
            {
                const string value = "value";
                var detector = new ReplayDetector<string>(TimeSpan.FromMilliseconds(10));
                detector.IsNew(value);
                Assert.IsFalse(detector.IsNew(value));
            }

            [TestMethod]
            public void SaysOldThingIsNewAfterWindowPasses()
            {
                const string value = "value";
                var window = TimeSpan.FromMilliseconds(100);
                var detector = new ReplayDetector<string>(window);
                detector.IsNew(value);
                Thread.Sleep(window + window); // Additional time to reduce chance of race condition
                Assert.IsTrue(detector.IsNew(value));
            }

            [TestMethod]
            public void KeepsOldThingOldWhenRepeatedlyChecked()
            {
                const string value = "value";
                var window = TimeSpan.FromMilliseconds(100);
                var halfWindow = TimeSpan.FromTicks(window.Ticks / 2);
                var detector = new ReplayDetector<string>(window);
                detector.IsNew(value);
                for (var i = 0; i < 10; ++i)
                {
                    Thread.Sleep(halfWindow);
                    Assert.IsFalse(detector.IsNew(value));
                }
            }
        }
    }
}
