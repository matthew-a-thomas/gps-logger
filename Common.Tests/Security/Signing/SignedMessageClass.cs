using Common.Security.Signing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Tests.Security.Signing
{
    [TestClass]
    public class SignedMessageClass
    {
        private static IReadOnlyList<SignedMessage<bool>> CreateDuplicates(int count) => Enumerable.Repeat(0, count).Select(x => new SignedMessage<bool> { Contents = false, HMAC = "hmac", ID = "id", Salt = "salt", UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds() }).ToList();

        [TestClass]
        public class GetHashCodeMethod
        {
            [TestMethod]
            public void TwoInstancesWithSameValuesHaveSameHashCode()
            {
                var instances = CreateDuplicates(2);
                var hashCode = instances.FirstOrDefault()?.GetHashCode();
                foreach (var otherInstance in instances.Skip(1))
                {
                    Assert.AreEqual(hashCode, otherInstance.GetHashCode());
                }
            }
        }

        [TestClass]
        public class EqualsMethod
        {
            [TestMethod]
            public void TwoInstancesWithSameValuesAreEqual()
            {
                var instances = CreateDuplicates(2);
                foreach (var instance in instances)
                {
                    foreach (var otherInstance in instances.Except(new[] { instance }))
                    {
                        Assert.IsTrue(instance.Equals(otherInstance));
                    }
                }
            }
        }
    }
}
