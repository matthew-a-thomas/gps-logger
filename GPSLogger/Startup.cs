﻿using System;
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

            // Register the composition root
            builder.RegisterModule<CompositionRoot>();

            // Build an Autofac container
            var container = builder.Build();

            // Assert that everything looks cool with this container
            AssertSanityChecks(container);

            // Return a new service provider for this container
            return new AutofacServiceProvider(container);
        }
    }
}
