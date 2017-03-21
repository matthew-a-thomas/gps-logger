using Autofac;
using SQLDatabase.RemoteStorage.Query;
using Common.RemoteStorage.Query;

namespace SQLDatabase
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocationProvider>().As<ILocationProvider>();
        }
    }
}
