using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GPSLogger.Integration
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ArrangeTypeModifiers
    class Program
    {
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once SuggestBaseTypeForParameter
        static void Main(string[] args)
        {
            Console.WriteLine(JsonConvert.SerializeObject(args));
            var program = new Program();
            program.RunTests().Wait();
        }

        private async Task RunTests()
        {
            var server = await Helpers.CreateServerAsync();
        }
    }
}