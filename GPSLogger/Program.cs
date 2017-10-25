using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GPSLogger
{
    using System;
    using Microsoft.AspNetCore;

    public class Program
    {
        public static void Main(string[] args)
        {
            GC.KeepAlive(typeof(Microsoft.AspNetCore.Mvc.Razor.Host.ViewComponentTagHelperDescriptorConventions));
            
            var host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

            host.Run();
        }
    }
}
