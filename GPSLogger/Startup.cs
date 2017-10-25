using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Runtime.Loader;
using Autofac.Core;

namespace GPSLogger
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Verifies that certain required types can be resolved from Autofac
        /// </summary>
        /// <param name="container"></param>
        private static void AssertSanityChecks(IContainer container)
        {
            // Verify that all controllers can be created from Autofac
            using (var scope = container.BeginLifetimeScope())
            {
                var unregisteredTypes =
                    Assembly
                    .GetEntryAssembly()
                    .GetTypes()
                    .Where(type => typeof(Controller).IsAssignableFrom(type))
                    .Select(type =>
                    {
                        try
                        {
                            if (!scope.IsRegistered(type))
                                return new { Error = new Exception("Cannot resolve type"), Type = type };
                            scope.Resolve(type);
                            return null;
                        }
                        catch (Exception e)
                        {
                            return new { Error = e, Type = type };
                        }
                    })
                    .Where(x => !ReferenceEquals(x, null))
                    .ToList();

                if (unregisteredTypes.Any())
                    throw new Exception(
                        string.Join(
                            Environment.NewLine + Environment.NewLine,
                            new[] { "These controllers cannot be created:" }
                            .Concat(unregisteredTypes.Select(x =>
                            {
                                var error = x.Error;
                                while (!ReferenceEquals(error.InnerException, null))
                                    error = error.InnerException;
                                return x.Type.FullName + " - " + error.Message;
                            }))
                        )
                    );
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseStaticFiles();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Enable CORS
            services.AddCors();

            // Add framework services.
            services.AddMvc();
            
            // Autofac dependency injection
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_hostingEnvironment).SingleInstance();

            // Add services to Autofac
            builder.Populate(services);

            // Use MEF to search for all Autofac IModules
            RegisterModules(builder);

            // Build an Autofac container
            var container = builder.Build();

            // Assert that everything looks cool with this container
            AssertSanityChecks(container);

            // Return a new service provider for this container
            return new AutofacServiceProvider(container);
        }

        /// <summary>
        /// Registers all Autofac IModules.
        /// Finds them using MEF
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterModules(ContainerBuilder builder)
        {
            // Create a ConventionBuilder
            var conventions = new ConventionBuilder();

            // Tell the ConventionBuilder that we're looking for things that export IModule
            conventions
                .ForTypesDerivedFrom<IModule>()
                .Export<IModule>()
                .Shared();

            // Find all assemblies around the current assembly
            var thisType = typeof(Startup);
            var directoryHavingThisAssembly = new FileInfo(thisType.GetTypeInfo().Assembly.Location).Directory.FullName;
            var assemblies = Directory
                .GetFiles(directoryHavingThisAssembly, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(AssemblyLoadContext.GetAssemblyName)
                .Select(name =>
                {
                    try
                    {
                        return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(x => !ReferenceEquals(null, x))
                .Distinct()
                .ToList();

            // Set up MEF configuration
            var configuration = new ContainerConfiguration()
                .WithAssemblies(
                assemblies,
                conventions
            );

            // Create a MEF container to grab IModules from
            using (var mefContainer = configuration.CreateContainer())
            {
                // Grab all registered IModules
                var modules = mefContainer.GetExports<IModule>();

                // Register each of these with Autofac
                foreach (var module in modules)
                    builder.RegisterModule(module);
            }
        }
    }
}
