using Common.Extensions;
using Common.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class IFactoryExtensionsClass
    {
        [TestClass]
        public class ChainIntoMethod
        {
            [TestMethod]
            public void HandlesNopInterfaces()
            {
                // ReSharper disable once InvokeAsExtensionMethod
                IFactoryExtensions.ChainInto<object, object, object>(
                    new Mock<IFactory<object, object>>().Object,
                    new Mock<IFactory<object, object>>().Object
                );
            }

            [TestMethod]
            public void HandlesNullParameters()
            {
                IFactoryExtensions.ChainInto<object, object, object>(null, null);
                // ReSharper disable once InvokeAsExtensionMethod
                IFactoryExtensions.ChainInto<object, object, object>(new Factory<object, object>(_ => null), null);
                IFactoryExtensions.ChainInto<object, object, object>(null, new Factory<object, object>(_ => null));
            }
        }
    }
}
