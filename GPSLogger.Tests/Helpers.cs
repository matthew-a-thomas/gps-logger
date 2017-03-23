using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Net.Http;
using Common.Security.Signing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace GPSLogger.Tests
{
    public static class Helpers
    {
        public static async Task AssertNoPropertiesAreNullAsync(object o)
        {
            await Task.Run(() => Assert.IsTrue(ReflectPropertiesOf<string>(o).All(tuple => !string.IsNullOrWhiteSpace(tuple.Item2))));
        }

        /// <summary>
        /// Asserts that what comes back from the given URL is valid JSON
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        // ReSharper disable once InconsistentNaming
        public static async Task AssertReturnsJSONAsync(TestServer server, string url)
        {
            JsonConvert.DeserializeObject(await GetAsync(server, url));
        }

        public static void AssertIsNotSigned<T>(SignedMessage<T> signedMessage)
        {
            var threw = false;
            try
            {
                AssertSigningFieldsAreNonNull(signedMessage);
            }
            catch
            {
                threw = true;
            }
            Assert.IsTrue(threw);
        }

        public static void AssertSigningFieldsAreNonNull<T>(SignedMessage<T> signedMessage)
        {
            Assert.IsNotNull(signedMessage.HMAC);
            Assert.IsNotNull(signedMessage.ID);
            Assert.IsNotNull(signedMessage.Salt);
        }
    
        /// <summary>
        /// Creates a new GPS Logger server
        /// </summary>
        /// <returns></returns>
        public static async Task<TestServer> CreateServerAsync()
        {
            // Figure out where the GPS Logger directory is
            var startingLocation = new DirectoryInfo(Path.GetDirectoryName(typeof(Helpers).GetTypeInfo().Assembly.Location));
            var baseDirectory = Path.Combine(startingLocation.Parent.Parent.Parent.Parent.FullName, typeof(Startup).Namespace);

            // Create a temp directory for this server to run from
            var temp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " - " + Guid.NewGuid().ToString();
            var contentRoot = Path.Combine(baseDirectory, "tests", temp);
            var contentRootDirectory = Directory.CreateDirectory(contentRoot);

            // Clean up older files on a background thread
            var pattern = new Regex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}-[0-9]{2}-[0-9]{2} - [a-f0-9\-]+$");
            Assert.IsTrue(pattern.IsMatch(contentRootDirectory.Name));
            var maxAge = TimeSpan.FromDays(1);
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                foreach (var directory in
                    contentRootDirectory
                    .Parent
                    .EnumerateDirectories()
                    .Where(directory => pattern.IsMatch(directory.Name))
                    .Where(directory => DateTimeOffset.Now - directory.CreationTimeUtc > maxAge))
                {
                    try
                    {
                        directory.Delete(true);
                    }
                    catch { }
                }
            });

            return await Task.Run(() =>
            {
                // Copy over needed files
                foreach (var file in new[]
                {
                    "appsettings.json"
                })
                    File.Copy(Path.Combine(baseDirectory, file), Path.Combine(contentRoot, file));

                // Spin up the server
                return new TestServer(
                    new WebHostBuilder()
                    .UseContentRoot(contentRoot)
                    .UseStartup<Startup>()
                    );
            });
        }

        /// <summary>
        /// Recursively reflects the given object, pulling out all public instance properties as "name=value" pairs
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> CreateUrlParametersFromAsync(object o)
            => await Task.Run(() => ReflectPropertiesOf<string>(o).Select(tuple => tuple.Item1.Name + "=" + WebUtility.UrlEncode(tuple.Item2)));

        /// <summary>
        /// Returns the string the came back from getting the given URL from the given server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(TestServer server, string url)
        {
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
        }

        public static async Task<string> PostAsync(TestServer server, string url, object contents)
        {
            using (var client = server.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = JsonConvert.SerializeObject(contents);
                var encodedContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, encodedContent);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
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
