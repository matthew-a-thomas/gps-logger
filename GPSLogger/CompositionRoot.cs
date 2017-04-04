using Autofac;
using Common.Extensions;
using Common.LocalStorage;
using Common.Messages;
using Common.RemoteStorage.Command;
using Common.RemoteStorage.Query;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;
using Common.Utilities;
using GPSLogger.Implementations;
using GPSLogger.Interfaces;
using GPSLogger.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GPSLogger.Utilities;

namespace GPSLogger
{
    public class CompositionRoot : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            { // Security delegates
              // GenerateSaltDelegate
                builder.Register(c =>
                {
                    var rngFactoryAsync = c.Resolve<Delegates.RNGFactoryAsync>();
                    var keySizeProvider = c.Resolve<KeySizeProvider>();
                    return new Delegates.GenerateSaltDelegateAsync(async () =>
                    {
                        using (var rng = await rngFactoryAsync())
                        {
                            return await rng.GetBytesAsync(keySizeProvider.KeySize);
                        }
                    });
                });

                // GenerateCredentialDelegate
                builder.Register(c =>
                {
                    var hmacProvider = c.Resolve<IKeyedHMACProvider>();
                    return new Delegates.GenerateCredentialDelegateAsync(async id =>
                    {
                        id = id ?? new byte[0];
                        using (var hmac = await hmacProvider.GetAsync())
                        {
                            var secret = hmac.ComputeHash(id);
                            return new Credential<byte[]> { ID = id.CreateClone(), Secret = secret };
                        }
                    });
                });
                
                // HMAC stuff
                builder.RegisterType<KeyedHMACProvider>().As<IKeyedHMACProvider>().SingleInstance();
                builder.RegisterType<HMACProvider>().As<IHMACProvider>().SingleInstance();
                builder.RegisterType<HMACKey>().WithParameter(new NamedParameter("keyName", "hmac key")).As<IHMACKey>().SingleInstance();
                builder.Register(c =>
                    {
                        var hmacProvider = c.Resolve<IHMACProvider>();
                        using (var hmac = hmacProvider.GetAsync(new byte[0]).WaitAndGet())
                        {
                            var keySize = hmac.HashSize / 8;
                            return (IKeySizeProvider) new KeySizeProvider(keySize);
                        }
                    })
                .SingleInstance();

                // RNG factory
                builder.RegisterInstance(new Delegates.RNGFactoryAsync(() => Task.FromResult(RandomNumberGenerator.Create()))); // Not single instance, since we need a new RNG each time
            }

            // App_Data directory
            builder.Register(c =>
            {
                var environment = c.Resolve<IHostingEnvironment>();
                var root = new DirectoryInfo(Path.Combine(environment.ContentRootPath, "App_Data"));
                root.Create();
                return root;
            });

            // App_Data file provider
            builder.Register(c =>
            {
                var appData = c.Resolve<DirectoryInfo>();
                IFileProvider fileProvider = new PhysicalFileProvider(appData.FullName);
                return fileProvider;
            }).SingleInstance();

            // IStorage
            builder.Register(c =>
            {
                var appData = c.Resolve<DirectoryInfo>();
                const int maxKeyLength = 100;
                return (IStorage<byte[]>)new PhysicalStorage(appData, maxKeyLength);
            }).SingleInstance();
            
            { // Controllers
                // IActionResultProducer
                builder.RegisterType<ActionResultProducer>().As<IActionResultProducer>();

                // Location storage and retrieval
                builder.Register(c =>
                {
                    var locationPoster = c.Resolve<ILocationPoster>();

                    return new LocationImpl.HandleLocationPostAsync(async (id, location) =>
                    {
                        await locationPoster.PostLocationAsync(id, new Common.RemoteStorage.Models.Location
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude
                        });
                    });
                });
                builder.Register(c =>
                {
                    var locationProvider = c.Resolve<ILocationProvider>();

                    return new LocationImpl.LocationProviderAsync(async id =>
                    {
                        var identifiedLocations = await locationProvider.GetAllLocationsAsync(id);
                        var locations = identifiedLocations.Select(x => new Common.RemoteStorage.Models.Location { Latitude = x.Latitude, Longitude = x.Longitude, UnixTime = x.UnixTime });
                        return locations;
                    });
                });

                // Location serializers
                ISerializer<Location> locationSerializer;
                {
                    var serializer = new Serializer<Location>();
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.Latitude));
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.Longitude));
                    locationSerializer = serializer;
                }
                builder.RegisterInstance(locationSerializer).SingleInstance();
                ISerializer<Common.RemoteStorage.Models.Location> remoteLocationSerializer;
                {
                    var serializer = new Serializer<Common.RemoteStorage.Models.Location>();
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.Latitude));
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.Longitude));
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.UnixTime));
                    remoteLocationSerializer = serializer;
                }
                builder.RegisterInstance(remoteLocationSerializer).SingleInstance();

                // Credential serializer
                builder.Register(c =>
                {
                    var serializer = new Serializer<Credential<byte[]>>();
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.ID));
                    serializer.EnqueueStepAsync(x => Task.FromResult(x.Secret));
                    return (ISerializer<Credential<byte[]>>)serializer;
                }).SingleInstance();

                // Credential implementation
                builder.RegisterType<CredentialImpl>().As<ICredential>().SingleInstance();
                
                // Location implementation
                builder.RegisterType<LocationImpl>().As<ILocation>().SingleInstance();

                // Time implementation
                builder.RegisterType<Time>().As<ITime>().SingleInstance();

                // Message handlers, validators, and signers
                {
                    // "bool" requests leading to "long" responses
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        Serializer<bool>.CreatePassthroughSerializer(),
                        Serializer<long>.CreatePassthroughSerializer()
                        );

                    // "Location" requests leading to "bool" responses
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        locationSerializer,
                        Serializer<bool>.CreatePassthroughSerializer()
                        );

                    // "bool" requests leading to "Credential" responses
                    var credentialSerializer = new Serializer<Credential<string>>();
                    credentialSerializer.EnqueueStepAsync(async x => await ByteArrayExtensions.FromHexStringAsync(x.ID));
                    credentialSerializer.EnqueueStepAsync(async x => await ByteArrayExtensions.FromHexStringAsync(x.Secret));
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        Serializer<bool>.CreatePassthroughSerializer(),
                        credentialSerializer
                        );
                }

                // Register all controllers
                var assembly = typeof(CompositionRoot).GetTypeInfo().Assembly;
                var types = assembly.GetTypes();
                foreach (var type in types
                    .Where(type => typeof(ControllerBase).IsAssignableFrom(type)))
                {
                    builder.RegisterType(type);
                }
            }
        }

        private static void RegisterHandlerValidatorAndSigner<TRequest, TResponse>(
            ContainerBuilder builder,
            ISerializer<TRequest> requestContentSerializer,
            ISerializer<TResponse> responseContentSerializer)
        {
            builder.RegisterType<MessageHandler<TRequest, TResponse>>().As<IMessageHandler<TRequest, TResponse>>().SingleInstance();
            builder.RegisterType<Validator<TRequest>>().SingleInstance();
            var slidingWindow = TimeSpan.FromMinutes(1);
            builder.RegisterInstance(new ReplayDetector<Message<TRequest>>(new TimeSpan(slidingWindow.Ticks * 2))).SingleInstance();
            builder.RegisterInstance(new Validator<TRequest>.PassesDomainSpecificValidationDelegateAsync(message => Task.Run(() =>
            {
                // Domain-specific validation to tell if a SignedMessage<TRequest> is valid

                // It isn't valid if the timestamps are too different
                var minutesOff = (DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(message.UnixTime)).TotalMinutes;
                if (Math.Abs(minutesOff) >= slidingWindow.TotalMinutes)
                    return false;

                // It isn't valid if the hex string fields aren't valid hex strings
                return new[] { message.ID, message.Salt }.All(x =>
                {
                    try
                    {
                        ByteArrayExtensions.FromHexStringAsync(x).Wait();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }))).SingleInstance();
            builder.RegisterInstance(new Validator<TRequest>.DeriveIDFromThingDelegateAsync(message => ByteArrayExtensions.FromHexStringAsync(message?.ID))); // Function that pulls the ID out of a message so that the signers/validators will know what ID to use

            RegisterSignerAndSerializers(builder, requestContentSerializer);
            RegisterSignerAndSerializers(builder, responseContentSerializer);
        }

        private static void RegisterSignerAndSerializers<T>(
            ContainerBuilder builder,
            ISerializer<T> contentSerializer)
        {
            builder.RegisterType<Signer<T>>().SingleInstance();
            builder.RegisterType<MessageSerializer<T>>().As<ISerializer<Message<T>>>().SingleInstance();
            builder.RegisterInstance(contentSerializer).SingleInstance();
        }
    }
}