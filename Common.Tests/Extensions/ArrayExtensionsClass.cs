using System.Linq;
using Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class ArrayExtensionsClass
    {
        [TestClass]
        public class CreateCloneMethod
        {
            [TestMethod]
            public void ReturnsDifferentReference()
            {
                var a = new[] {0, 1, 2};
                var b = a.CreateClone();
                Assert.IsFalse(ReferenceEquals(a, b));
            }

            [TestMethod]
            public void ReturnsExactCopy()
            {
                var a = Enumerable.Range(0, 10).ToArray();
                var b = a.CreateClone();
                Assert.IsTrue(a.SequenceEqual(b));
            }
        }
    }
}
