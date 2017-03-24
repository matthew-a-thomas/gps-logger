using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace SQLDatabase
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConfigurationFactory
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IConfiguration CreateConfiguration()
        {
            var thisAssemblyLocation = GetType().GetTypeInfo().Assembly.Location;
            var root = new FileInfo(thisAssemblyLocation).Directory.Root.FullName;
            const string sqlJsonName = "sql.json";

            var config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = sqlJsonName, Optional = true, FileProvider = new PhysicalFileProvider(root) })
                .Add(new EnvironmentVariablesConfigurationSource { Prefix = "SQL_" })
                .Build();
            return config;
        }
    }
}
