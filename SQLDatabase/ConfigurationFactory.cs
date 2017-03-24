using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace SQLDatabase
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConfigurationFactory
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IConfiguration CreateConfiguration()
        {
            var config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "sql.json", Optional = true, FileProvider = new PhysicalFileProvider(@"C:\") })
                .Add(new EnvironmentVariablesConfigurationSource { Prefix = "SQL_" })
                .Build();
            return config;
        }
    }
}
