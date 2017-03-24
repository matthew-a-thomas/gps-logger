using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
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
                var factory = new ConfigurationFactory(Enumerable.Empty<IConfigurationSource>());
                factory.CreateConfiguration();
            }

            [TestMethod]
            public void ReturnsSomethingWithValues()
            {
                var factory = new ConfigurationFactory(new IConfigurationSource[] { new MemoryConfigurationSource { InitialData = new[]
                {
                    new KeyValuePair<string, string>("hello", "world")
                } } });
                var config = factory.CreateConfiguration();
                var children = config.GetChildren();
                Assert.IsNotNull(children);
                Assert.IsTrue(children.Any());
            }
        }
    }
}
