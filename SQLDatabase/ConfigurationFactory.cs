using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SQLDatabase
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigurationFactory
    {
        private readonly ConfigurationBuilder _builder;

        public ConfigurationFactory(IEnumerable<IConfigurationSource> configSources)
        {
            _builder = new ConfigurationBuilder();
            foreach (var source in configSources)
            {
                _builder.Add(source);
            }
        }
        
        public IConfiguration CreateConfiguration() => _builder.Build();
    }
}
