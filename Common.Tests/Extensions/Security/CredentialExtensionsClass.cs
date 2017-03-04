using System.Reflection;
using System.Threading;
using Common.Extensions.Security;
using Common.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions.Security
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
