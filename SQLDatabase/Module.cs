using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;
using SQLDatabase.RemoteStorage.Command;
using Common.RemoteStorage.Command;
using System.Composition;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SQLDatabase
{
    [Export(typeof(IModule))]
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            IConfiguration config = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "sql.json", Optional = false })
                .Build();
            builder.RegisterInstance(config);

            builder.RegisterType<ConnectionProvider>();
            builder.RegisterType<Transaction>();
            builder.RegisterType<IdentifierPoster>();
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
            builder.RegisterType<LocationPoster>().As<ILocationPoster>();
        }
    }
}
