using System;
using Newtonsoft.Json;

namespace GPSLogger.Integration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(JsonConvert.SerializeObject(args));
        }
    }
}