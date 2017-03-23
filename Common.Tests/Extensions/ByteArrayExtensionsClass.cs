using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
            public async Task ReturnsNullWhenGivenNull()
            {
                var b = await ByteArrayExtensions.FromHexStringAsync(null);
                Assert.IsNull(b);
            }

            [TestMethod]
            public async Task ReturnsNullWhenGivenInvalidHexString()
            {
                var b = await ByteArrayExtensions.FromHexStringAsync("hello world");
                Assert.IsNull(b);
            }

            [TestMethod]
            public async Task HandlesHalfNibbleStrings()
            {
                var b = await ByteArrayExtensions.FromHexStringAsync("012");
                Assert.IsNotNull(b);
                Assert.AreEqual(1, b.Length);
                Assert.AreEqual(1, b[0]);
            }

            [TestMethod]
            public async Task HandlesAllHexDigits()
            {
                var allDigits =
                    string.Join("",
                        Enumerable.Range(0, 10).Select(x => x.ToString()[0])
                            .Concat(Enumerable.Range('a', 'f' - 'a' + 1).Select(x => (char) x))
                    );
                allDigits += allDigits.ToUpper();
                var b = await ByteArrayExtensions.FromHexStringAsync(allDigits);
                Assert.IsNotNull(b);
            }

            [TestMethod]
            public async Task DoesNotHandleNonHexDigits()
            {
                var nonHexDigits =
                    string.Join("",
                        Enumerable.Range(1, byte.MaxValue).Select(x => (char) x)
                            .Except(Enumerable.Range(0, 10).Select(x => x.ToString()[0])
                                .Concat(Enumerable.Range('a', 'f' - 'a' + 1).Select(x => (char) x)))
                    );
                var b = await ByteArrayExtensions.FromHexStringAsync(nonHexDigits);
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
        public async Task ToAndFromWorkTogether()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                for (var i = 0; i < 1000; ++i)
                {
                    var bytes = rng.GetBytes(16);
                    var s = bytes.ToHexString();
                    var b = await ByteArrayExtensions.FromHexStringAsync(s);
                    Assert.IsTrue(bytes.SequenceEqual(b));
                }
            }
        }
    }
}
