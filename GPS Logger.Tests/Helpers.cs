using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

namespace GPS_Logger.Tests
{
    public static class Helpers
    {
        /// <summary>
        /// Asserts that what comes back from the given URL is valid JSON
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        // ReSharper disable once InconsistentNaming
        public static void AssertReturnsJSON(TestServer server, string url)
        {
            JsonConvert.DeserializeObject(Get(server, url));
        }
    
        /// <summary>
        /// Creates a new GPS Logger server
        /// </summary>
        /// <returns></returns>
        public static TestServer CreateServer()
        {
            return new TestServer(
                new WebHostBuilder()
                .UseContentRoot("../../../../GPS Logger")
                .UseStartup<Startup>()
                );
        }

        /// <summary>
        /// Recursively reflects the given object, pulling out all public instance properties as "name=value" pairs
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IEnumerable<string> CreateUrlParametersFrom(object o)
            => ReflectPropertiesOf<string>(o).Select(tuple => tuple.Item1.Name + "=" + WebUtility.UrlEncode(tuple.Item2));

        /// <summary>
        /// Returns the string the came back from getting the given URL from the given server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Get(TestServer server, string url)
        {
            using (var client = server.CreateClient())
            {
                var responseTask = client.GetAsync(url);
                responseTask.Wait();
                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();
                var responseStringTask = response.Content.ReadAsStringAsync();
                responseStringTask.Wait();
                var responseString = responseStringTask.Result;
                return responseString;
            }
        }

        public static IEnumerable<Tuple<PropertyInfo, T>> ReflectPropertiesOf<T>(object o)
        {
            foreach (var property in
                o
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                var propertyValue = property.GetValue(o);
                var convertedValue = default(T);
                var success = false;
                try
                {
                    convertedValue = (T)Convert.ChangeType(propertyValue, typeof(T));
                    success = true;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }

                if (success)
                    yield return Tuple.Create(property, convertedValue);
                else
                    foreach (var thing in ReflectPropertiesOf<T>(propertyValue))
                        yield return thing;
            }
        }
    }
}
