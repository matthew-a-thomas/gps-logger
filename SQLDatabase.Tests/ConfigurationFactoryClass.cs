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
            public void ReturnsValuesFromSources()
            {
                var data = new[]
                {
                    new KeyValuePair<string, string>("hello", "world"),
                    new KeyValuePair<string, string>("goodbye", "moon")
                };
                var factory = new ConfigurationFactory(new IConfigurationSource[] { new MemoryConfigurationSource { InitialData = data} });
                var config = factory.CreateConfiguration();
                var children = config.GetChildren();
                Assert.IsNotNull(children, $"{nameof(children)} is null");
                Assert.IsTrue(children.Any(), $"{nameof(children)} is empty");
                Assert.IsTrue(config.AsEnumerable().Zip(data, (one, other) => one.Key == other.Key && one.Value == other.Value).Aggregate(true, (a, b) => a && b), $"{nameof(config)} doesn't return name values we put in");
            }
        }
    }
}
