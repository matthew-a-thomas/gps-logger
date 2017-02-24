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
using GPS_Logger.Security.Messages;
using GPS_Logger.Security.Messages.Signing;
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
                    var hmacFactory = c.Resolve<Delegates.HMACFactory>();
                    return new Delegates.GenerateCredentialDelegate(id =>
                    {
                        using (var hmac = hmacFactory())
                        {
                            var secret = hmac.ComputeHash(id);
                            return new Credential { ID = id.ToHexString(), Secret = secret.ToHexString() };
                        }
                    });
                });

                // HMACFactory
                builder.Register(c =>
                {
                    var keyController = c.Resolve<HMACKeyController>();
                    return new Delegates.HMACFactory(() => new HMACMD5(keyController.GetCurrent()));
                }); // Not single instance, since we need a new HMAC each time

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

            // Message signing/validating
            builder.RegisterType<MessageSigner>().SingleInstance();
            builder.RegisterType<MessageValidator>().SingleInstance();

            { // Controllers
                // Location storage
                var locations = new ConcurrentDictionary<string, ConcurrentQueue<Location>>();
                var listGetter = new Func<string, ConcurrentQueue<Location>>(id => locations.GetOrAdd(id, x => new ConcurrentQueue<Location>()));
                builder.RegisterInstance(new LocationController.HandleLocationPost((id, location) => listGetter(id).Enqueue(location))).SingleInstance();
                builder.RegisterInstance(new LocationController.LocationProvider(id => listGetter(id))).SingleInstance();

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
                    serializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.ID));
                    serializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.Secret));
                    return (ISerializer<Credential>)serializer;
                });

                // Message handler
                builder.RegisterType<MessageHandler>().SingleInstance(); // We only need one

                // Controllers
                builder.RegisterType<CredentialController>();
                builder.RegisterType<EpochController>();
                builder.RegisterType<HMACKeyController>();
                builder.RegisterType<LocationController>();
            }
        }
    }
}