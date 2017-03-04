using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class RandomNumberGeneratorExtensionsClass
    {
        [TestClass]
        public class GetBytesMethod
        {
            [TestMethod]
            public void ReturnsAsManyBytesAsRequested()
            {
                using (var rng = RandomNumberGenerator.Create())
                for (var i = 0; i < 1000; ++i)
                {
                    var bytes = rng.GetBytes(i);
                    Assert.AreEqual(i, bytes.Length);
                }
            }

            [TestMethod]
            public void ReturnsNullWhenGivenNegativeNumber()
            {
                using (var rng = RandomNumberGenerator.Create())
                    Assert.IsNull(rng.GetBytes(-1));
            }

            [TestMethod]
            public void ReturnsDifferentBytesEachTime()
            {
                var hash = new HashSet<uint>();
                using (var rng = RandomNumberGenerator.Create())
                {
                    for (var i = 0; i < 1000; ++i)
                    {
                        var bytes = rng.GetBytes(4);
                        var integer = BitConverter.ToUInt32(bytes, 0);
                        Assert.IsTrue(hash.Add(integer));
                    }
                }
            }
        }
    }
}
