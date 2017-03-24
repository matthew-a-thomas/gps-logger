using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;

namespace SQLDatabase
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConfigurationFactory
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IConfiguration CreateConfiguration()
        {
            var config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "sql.json", Optional = true })
                .Add(new EnvironmentVariablesConfigurationSource { Prefix = "SQL_" })
                .Build();
            return config;
        }
    }
}
