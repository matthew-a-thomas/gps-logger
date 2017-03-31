using System;
using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;
using SQLDatabase.RemoteStorage.Command;
using Common.RemoteStorage.Command;
using System.Composition;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Common.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Common.Extensions;

namespace SQLDatabase
{
    [Export(typeof(IModule))]
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommandLineArgumentsProvider>().As<IArgumentsProvider>().SingleInstance();
            builder.Register(c =>
            {
                var fileProvider = c.Resolve<IFileProvider>();
                const string sqlJsonName = "sql.json";

                var argumentsProvider = c.Resolve<IArgumentsProvider>();
                var arguments = argumentsProvider.GetArguments();

                var configSources = new IConfigurationSource[]
                {
                    // Try loading config values from a "sql.json" file located inside the registered IFileProvider
                    new JsonConfigurationSource { Path = sqlJsonName, Optional = true, FileProvider = fileProvider },
                    // Try loading config values from command line arguments
                    new CommandLineConfigurationSource { Args = arguments },
                    // Try loading config values from environment variables
                    new EnvironmentVariablesConfigurationSource { Prefix = "SQL_" }
                };

                var factory = new ConfigurationFactory(configSources);

                return factory;
            }).SingleInstance();
            builder.Register(c =>
            {
                var factory = c.Resolve<ConfigurationFactory>();
                var configuration = factory.CreateConfiguration();
                return configuration;
            }).SingleInstance();
            builder.Register(c =>
            {
                var configuration = c.Resolve<IConfiguration>();
                var connectionOptions = new ConnectionOptions
                {
                    Database = configuration["database"],
                    Password = configuration["password"],
                    Server = configuration["server"],
                    User = configuration["user"]
                };
                if (connectionOptions.GetType().GetProperties().Select(property => property.GetValue(connectionOptions)).Any(x => ReferenceEquals(x, null)))
                    throw new Exception("At least one of the connection options is null. Make sure all the options are specified in configuration");
                return connectionOptions;
            });
            builder.Register(c =>
            {
                IFactory<ConnectionOptions, string> connectionStringFactory = new Factory<ConnectionOptions, string>(options => new SqlConnectionStringBuilder
                {
                    UserID = options.User,
                    Password = options.Password,
                    DataSource = options.Server,
                    IntegratedSecurity = false,
                    InitialCatalog = options.Database,
                    Encrypt = true
                }.ToString());
                return connectionStringFactory;
            }).SingleInstance();
            builder.Register(c =>
            {
                IFactory<string, SqlConnection> connectionFactory = new Factory<string, SqlConnection>(connectionString =>
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                });
                return connectionFactory;
            }).SingleInstance();
            builder.Register(c =>
            {
                var connectionStringFactory = c.Resolve<IFactory<ConnectionOptions, string>>();
                var connectionFactory = c.Resolve<IFactory<string, SqlConnection>>();
                return connectionStringFactory.ChainInto(connectionFactory);
            });
            builder.RegisterType<Transaction>().As<ITransaction>();
            builder.RegisterType<IdentifierPoster>().As<IIdentifierPoster>();
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
            builder.RegisterType<LocationPoster>().As<ILocationPoster>();
        }
    }
}
