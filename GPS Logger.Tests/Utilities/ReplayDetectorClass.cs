using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }
    }
}
