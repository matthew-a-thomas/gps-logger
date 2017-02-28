using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;

namespace GPS_Logger
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            
            // Autofac dependency injection
            var builder = new ContainerBuilder();
            builder.RegisterModule<CompositionRoot>();
            var container = builder.Build();

            // Verify that all controllers can be created from Autofac
            using (var scope = container.BeginLifetimeScope())
            {
                var unregisteredTypes =
                    Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => typeof(ApiController).IsAssignableFrom(type))
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
            
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    controller = "Credential",
                    id = RouteParameter.Optional
                }
            );

            // Make JSON serialization by default
            config.Formatters.Add(new BrowserJsonFormatter());
        }
    }
}
