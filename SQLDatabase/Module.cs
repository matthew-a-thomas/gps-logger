using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;
using SQLDatabase.RemoteStorage.Command;
using Common.RemoteStorage.Command;
using System.Composition;
using Autofac.Core;
using System.Data.SqlClient;
using System.IO;

namespace SQLDatabase
{
    [Export(typeof(IModule))]
    public class Module : Autofac.Module, IModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var connectionString = File.ReadAllText(@"C:\connection string.txt");
                var connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
            });
            builder.RegisterType<Transaction>();
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
            builder.RegisterType<LocationPoster>().As<ILocationPoster>();
        }
    }
}
