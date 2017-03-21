using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;
using SQLDatabase.RemoteStorage.Command;
using Common.RemoteStorage.Command;
using System.Composition;
using Common.RemoteStorage;

namespace SQLDatabase
{
    [Export(typeof(IRemoteStorageModule))]
    public class Module : Autofac.Module, IRemoteStorageModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
            builder.RegisterType<IdentifierPoster>().As<IIdentifierPoster>();
            builder.RegisterType<LocationPoster>().As<ILocationPoster>();
        }
    }
}
