using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using Autofac;
using GPS_Logger.Controllers;
using GPS_Logger.Extensions;
using GPS_Logger.LocalStorage;
using GPS_Logger.Models;
using GPS_Logger.Security;
using GPS_Logger.Serialization;

namespace GPS_Logger
{
    public class CompositionRoot : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            { // Security delegates
              // GenerateSaltDelegate
                builder.Register(c =>
                {
                    var rngFactory = c.Resolve<Delegates.RNGFactory>();
                    return new Delegates.GenerateSaltDelegate(() =>
                    {
                        using (var rng = rngFactory())
                        {
                            return rng.GetBytes(CredentialController.IDSize);
                        }
                    });
                });

                // GenerateCredentialDelegate
                builder.Register(c =>
                {
                    var hmacProvider = c.Resolve<IHMACProvider>();
                    return new Delegates.GenerateCredentialDelegate(id =>
                    {
                        using (var hmac = hmacProvider.Get())
                        {
                            var secret = hmac.ComputeHash(id);
                            return new Credential { ID = id.CreateClone(), Secret = secret };
                        }
                    });
                });

                // HMACKeyProvider
                builder.Register(c =>
                {
                    var controller = c.Resolve<HMACKeyController>();
                    return new Delegates.HMACKeyProvider(controller.GetCurrent);
                });

                // IHMACProvider
                builder.RegisterType<HMACProvider>().As<IHMACProvider>().SingleInstance();

                // RNG factory
                builder.RegisterInstance(new Delegates.RNGFactory(RandomNumberGenerator.Create)); // Not single instance, since we need a new RNG each time
            }

            // IPersistentStore
            builder.Register(c =>
            {
                var root = new DirectoryInfo(HostingEnvironment.MapPath("~/App_Data") ?? Guid.NewGuid().ToString());
                root.Create();
                return (IPersistentStore)new PersistentStore(root);
            }).SingleInstance();
            builder.RegisterType<PersistentStoreManager>().SingleInstance();
            
            { // Controllers
                // Location storage
                var locations = new ConcurrentDictionary<string, ConcurrentQueue<Location>>();
                var listGetter = new Func<string, ConcurrentQueue<Location>>(id => locations.GetOrAdd(id, x => new ConcurrentQueue<Location>()));
                builder.RegisterInstance(new LocationController.HandleLocationPost((id, location) => listGetter(id.ToHexString()).Enqueue(location))).SingleInstance();
                builder.RegisterInstance(new LocationController.LocationProvider(id => listGetter(id.ToHexString()))).SingleInstance();

                // Location serializer
                builder.Register(c =>
                {
                    var serializer = new Serializer<Location>();
                    serializer.EnqueueStep(x => x.Latitude);
                    serializer.EnqueueStep(x => x.Longitude);
                    return (ISerializer<Location>)serializer;
                }).SingleInstance();

                // Credential serializer
                builder.Register(c =>
                {
                    var serializer = new Serializer<Credential>();
                    serializer.EnqueueStep(x => x.ID);
                    serializer.EnqueueStep(x => x.Secret);
                    return (ISerializer<Credential>)serializer;
                });
                
                // Controllers
                builder.RegisterType<CredentialController>();
                builder.RegisterType<EpochController>();
                builder.RegisterType<HMACKeyController>();
                builder.RegisterType<LocationController>();
            }
        }
    }
}