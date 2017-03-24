using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLDatabase.Tests
{
    [TestClass]
    public class ConfigurationFactoryClass
    {
        [TestClass]
        public class CreateConfigurationMethod
        {
            [TestMethod]
            public void IsErrorFree()
            {
                var factory = new ConfigurationFactory();
                factory.CreateConfiguration();
            }

            [TestMethod]
            public void ReturnsSomethingWithValues()
            {
                var factory = new ConfigurationFactory();
                var config = factory.CreateConfiguration();
                var children = config.GetChildren();
                Assert.IsNotNull(children);
                Assert.IsTrue(children.Any());
            }
        }
    }
}
