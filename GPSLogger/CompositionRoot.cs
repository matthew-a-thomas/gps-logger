using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Autofac;
using Common.Extensions;
using Common.LocalStorage;
using Common.Messages;
using Common.Security;
using Common.Security.Signing;
using Common.Serialization;
using GPSLogger.Controllers;
using GPSLogger.Models;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Common.Utilities;
using Common.RemoteStorage.Command;
using Common.RemoteStorage.Query;

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
                    return new Delegates.GenerateSaltDelegateAsync(async () =>
                    {
                        using (var rng = await rngFactoryAsync())
                        {
                            return await rng.GetBytesAsync(CredentialController.IDSize);
                        }
                    });
                });

                // GenerateCredentialDelegate
                builder.Register(c =>
                {
                    var hmacProvider = c.Resolve<IHMACProvider>();
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

                // HMACKeyProvider
                builder.Register(c =>
                {
                    var controller = c.Resolve<HMACKeyController>();
                    return new Delegates.HMACKeyProviderAsync(controller.GetCurrentAsync);
                });

                // IHMACProvider
                builder.RegisterType<HMACProvider>().As<IHMACProvider>().SingleInstance();

                // RNG factory
                builder.RegisterInstance(new Delegates.RNGFactoryAsync(() => new ValueTask<RandomNumberGenerator>(RandomNumberGenerator.Create()))); // Not single instance, since we need a new RNG each time
            }

            // IPersistentStore
            builder.Register(c =>
            {
                var environment = c.Resolve<IHostingEnvironment>();
                var root = new DirectoryInfo(Path.Combine(environment.ContentRootPath, "App_Data"));
                root.Create();
                return (IPersistentStore)new PersistentStore(root);
            }).SingleInstance();
            
            { // Controllers
                // Location storage and retrieval
                builder.Register(c =>
                {
                    var locationPoster = c.Resolve<ILocationPoster>();

                    return new LocationController.HandleLocationPostAsync(async (id, location) =>
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

                    return new LocationController.LocationProviderAsync(async id =>
                    {
                        var identifiedLocations = await locationProvider.GetAllLocationsAsync(id);
                        var locations = identifiedLocations.Select(x => new Location { Latitude = x.Latitude, Longitude = x.Longitude });
                        return locations;
                    });
                });
                
                // Location serializer
                builder.Register(c =>
                {
                    var serializer = new Serializer<Location>();
                    serializer.EnqueueStepAsync(x => new ValueTask<double>(x.Latitude));
                    serializer.EnqueueStepAsync(x => new ValueTask<double>(x.Longitude));
                    return (ISerializer<Location>)serializer;
                }).SingleInstance();

                // Credential serializer
                builder.Register(c =>
                {
                    var serializer = new Serializer<Credential<byte[]>>();
                    serializer.EnqueueStepAsync(x => new ValueTask<byte[]>(x.ID));
                    serializer.EnqueueStepAsync(x => new ValueTask<byte[]>(x.Secret));
                    return (ISerializer<Credential<byte[]>>)serializer;
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
                    locationSerializer.EnqueueStepAsync(x => new ValueTask<double>(x.Latitude));
                    locationSerializer.EnqueueStepAsync(x => new ValueTask<double>(x.Longitude));
                    RegisterHandlerValidatorAndSigner(
                        builder,
                        locationSerializer,
                        Serializer<bool>.CreatePassthroughSerializer()
                        );

                    // "bool" requests leading to "Credential" responses
                    var credentialSerializer = new Serializer<Credential<string>>();
                    credentialSerializer.EnqueueStepAsync(x => new ValueTask<int>(x.ID?.Length ?? 0));
                    credentialSerializer.EnqueueStepAsync(x => new ValueTask<string>(x.ID));
                    credentialSerializer.EnqueueStepAsync(x => new ValueTask<int>(x.Secret?.Length ?? 0));
                    credentialSerializer.EnqueueStepAsync(x => new ValueTask<string>(x.Secret));
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
            builder.RegisterType<MapperTranslator<Message<TResponse>, SignedMessage<TResponse>>>().As<ITranslator<Message<TResponse>, SignedMessage<TResponse>>>();
            builder.RegisterType<MessageHandler<TRequest, TResponse>>().As<IMessageHandler<TRequest, TResponse>>().SingleInstance();
            builder.RegisterType<Validator<SignedMessage<TRequest>, Message<TRequest>>>().SingleInstance();
            var slidingWindow = TimeSpan.FromMinutes(1);
            builder.RegisterInstance(new ReplayDetector<SignedMessage<TRequest>>(new TimeSpan(slidingWindow.Ticks * 2))).SingleInstance();
            builder.RegisterInstance(new Validator<SignedMessage<TRequest>, Message<TRequest>>.PassesDomainSpecificValidationDelegateAsync(message => Task.Run(() =>
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
            builder.RegisterInstance(new Validator<SignedMessage<TRequest>, Message<TRequest>>.DeriveIDFromThingDelegateAsync(message => ByteArrayExtensions.FromHexStringAsync(message?.ID))); // Function that pulls the ID out of a message so that the signers/validators will know what ID to use

            RegisterSignerAndSerializers(builder, requestContentSerializer);
            RegisterSignerAndSerializers(builder, responseContentSerializer);
        }

        private static void RegisterSignerAndSerializers<T>(
            ContainerBuilder builder,
            ISerializer<T> contentSerializer)
        {
            builder.RegisterType<MapperTranslator<Message<T>, SignedMessage<T>>>().As<ITranslator<Message<T>, SignedMessage<T>>>();
            builder.RegisterType<Signer<SignedMessage<T>, Message<T>>>().SingleInstance();
            builder.RegisterType<MessageSerializer<T>>().As<ISerializer<Message<T>>>().SingleInstance();
            builder.RegisterInstance(contentSerializer).SingleInstance();
        }
    }
}