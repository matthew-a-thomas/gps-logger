using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPS_Logger.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPS_Logger.Tests.Extensions
{
    [TestClass]
    public class ByteArrayExtensionsClass
    {
        [TestClass]
        public class FromHexStringMethod
        {
            [TestMethod]
            public void ReturnsNullWhenGivenNumm()
            {
                var b = ByteArrayExtensions.FromHexString(null);
                Assert.IsNull(b);
            }
        }
    }
}
