using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GPSLogger
{
    using Microsoft.AspNetCore;

    public class Program
    {
        public static void Main(string[] args)
        {
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
