using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;
using SQLDatabase.RemoteStorage.Command;
using Common.RemoteStorage.Command;
using System.Composition;
using Autofac.Core;

namespace SQLDatabase
{
    [Export(typeof(IModule))]
    public class Module : Autofac.Module, IModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
            builder.RegisterType<LocationPoster>().As<ILocationPoster>();
        }
    }
}
