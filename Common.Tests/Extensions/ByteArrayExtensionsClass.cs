using System.Linq;
using System.Security.Cryptography;
using Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class ByteArrayExtensionsClass
    {
        [TestClass]
        public class FromHexStringMethod
        {
            [TestMethod]
            public void ReturnsNullWhenGivenNull()
            {
                var b = ByteArrayExtensions.FromHexString(null);
                Assert.IsNull(b);
            }

            [TestMethod]
            public void ReturnsNullWhenGivenInvalidHexString()
            {
                var b = ByteArrayExtensions.FromHexString("hello world");
                Assert.IsNull(b);
            }

            [TestMethod]
            public void HandlesHalfNibbleStrings()
            {
                var b = ByteArrayExtensions.FromHexString("012");
                Assert.IsNotNull(b);
                Assert.AreEqual(1, b.Length);
                Assert.AreEqual(1, b[0]);
            }

            [TestMethod]
            public void HandlesAllHexDigits()
            {
                var allDigits =
                    string.Join("",
                        Enumerable.Range(0, 10).Select(x => x.ToString()[0])
                            .Concat(Enumerable.Range('a', 'f' - 'a' + 1).Select(x => (char) x))
                    );
                allDigits += allDigits.ToUpper();
                var b = ByteArrayExtensions.FromHexString(allDigits);
                Assert.IsNotNull(b);
            }

            [TestMethod]
            public void DoesNotHandleNonHexDigits()
            {
                var nonHexDigits =
                    string.Join("",
                        Enumerable.Range(1, byte.MaxValue).Select(x => (char) x)
                            .Except(Enumerable.Range(0, 10).Select(x => x.ToString()[0])
                                .Concat(Enumerable.Range('a', 'f' - 'a' + 1).Select(x => (char) x)))
                    );
                var b = ByteArrayExtensions.FromHexString(nonHexDigits);
                Assert.IsNull(b);
            }
        }

        [TestClass]
        public class ToHexStringMethod
        {
            [TestMethod]
            public void MakesLowercasedStrings()
            {
                var test = Enumerable.Range(0, byte.MaxValue).Select(x => (byte)x).ToArray();
                var s = test.ToHexString();
                Assert.AreEqual(s.ToLower(), s);
            }

            [TestMethod]
            public void CanHandleRandomByteArrays()
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    for (var i = 0; i < 1000; ++i)
                    {
                        var bytes = rng.GetBytes(16);
                        var s = bytes.ToHexString();
                    }
                }
            }
        }

        [TestMethod]
        public void ToAndFromWorkTogether()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                for (var i = 0; i < 1000; ++i)
                {
                    var bytes = rng.GetBytes(16);
                    var s = bytes.ToHexString();
                    var b = ByteArrayExtensions.FromHexString(s);
                    Assert.IsTrue(bytes.SequenceEqual(b));
                }
            }
        }
    }
}
