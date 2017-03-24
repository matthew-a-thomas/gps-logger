using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
            public async Task InvokesConversionFunctionForAllMembers()
            {
                var credential = new Credential<string>();
                var numProperties =
                    credential
                    .GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Length;
                var numTimesCalled = 0;
                await credential.ConvertAsync(x => Task.FromResult(Interlocked.Increment(ref numTimesCalled)));
                Assert.AreEqual(numProperties, numTimesCalled);
            }
        }
    }
}
