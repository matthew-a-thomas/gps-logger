using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SQLDatabase
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfigurationFactory
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IConfiguration CreateConfiguration()
        {
            var config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "sql.json", Optional = false })
                .Build();
            return config;
        }
    }
}
