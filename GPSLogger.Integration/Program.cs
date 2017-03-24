using System;
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
            throw new NotImplementedException();
        }
    }
}