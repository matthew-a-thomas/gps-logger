using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPS_Logger.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPS_Logger.Extensions.Security;
using Moq;

namespace GPS_Logger.Tests.Extensions.Security
{
    [TestClass]
    public class CredentialExtensionsClass
    {
        [TestClass]
        public class ConvertMethod
        {
            [TestMethod]
            public void InvokesConversionFunctionForAllMembers()
            {
                var a = new Credential<string>();
                var numProperties =
                    a
                    .GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Length;
                var numTimesCalled = 0;
                a.Convert(x => Interlocked.Increment(ref numTimesCalled));
                Assert.AreEqual(numProperties, numTimesCalled);
            }
        }
    }
}
