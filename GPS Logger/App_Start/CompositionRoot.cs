using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Services.Description;
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

                // Message handlers, validators, and signers
                {
                    // "bool" requests leading to "long" responses
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        Serializer<bool>.CreatePassthroughSerializer(),
                        Serializer<long>.CreatePassthroughSerializer()
                        );

                    // "Location" requests leading to "bool" responses
                    var locationSerializer = new Serializer<Location>();
                    locationSerializer.EnqueueStep(x => x.Latitude);
                    locationSerializer.EnqueueStep(x => x.Longitude);
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        locationSerializer,
                        Serializer<bool>.CreatePassthroughSerializer()
                        );

                    // "bool" requests leading to "Credential" responses
                    var credentialSerializer = new Serializer<Credential>();
                    credentialSerializer.EnqueueStep(x => x.ID?.Length ?? 0);
                    credentialSerializer.EnqueueStep(x => x.ID);
                    credentialSerializer.EnqueueStep(x => x.Secret?.Length ?? 0);
                    credentialSerializer.EnqueueStep(x => x.Secret);
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        Serializer<bool>.CreatePassthroughSerializer(),
                        credentialSerializer
                        );
                }

                // Controllers
                builder.RegisterType<CredentialController>();
                builder.RegisterType<TimeController>();
                builder.RegisterType<HMACKeyController>();
                builder.RegisterType<LocationController>();
            }
        }

        private static void RegisterHandlerValidatorAndSigner<TRequest, TResponse>(
            ContainerBuilder builder,
            ISerializer<TRequest> requestContentSerializer,
            ISerializer<TResponse> responseContentSerializer)
        {
            builder.RegisterType<MessageHandler<TRequest, TResponse>>().SingleInstance();
            builder.RegisterType<Validator<SignedMessage<TRequest>, Message<TRequest>>>().SingleInstance();
            builder.RegisterInstance(new Func<SignedMessage<TRequest>, bool>(message =>
            {
                // Domain-specific validation to tell if a SignedMessage<TRequest> is valid

                // It isn't valid if the timestamps are too different
                var minutesOff = (DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(message.UnixTime)).TotalMinutes;
                if (Math.Abs(minutesOff) > 1.0)
                    return false;

                // It isn't valid if the hex string fields aren't valid hex strings
                return new[] { message.ID, message.Salt }.All(x =>
                {
                    try
                    {
                        ByteArrayExtensions.FromHexString(x);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
            })).SingleInstance();
            builder.RegisterInstance(new Func<SignedMessage<TRequest>, byte[]>(message => ByteArrayExtensions.FromHexString(message?.ID))); // Function that pulls the ID out of a message so that the signers/validators will know what ID to use

            RegisterSignerAndSerializers(builder, requestContentSerializer);
            RegisterSignerAndSerializers(builder, responseContentSerializer);
        }

        private static void RegisterSignerAndSerializers<T>(
            ContainerBuilder builder,
            ISerializer<T> contentSerializer)
        {
            builder.RegisterType<Signer<SignedMessage<T>, Message<T>>>().SingleInstance();
            builder.RegisterType<MessageSerializer<T>>().As<ISerializer<Message<T>>>().SingleInstance();
            builder.RegisterInstance(contentSerializer).SingleInstance();
        }
    }
}