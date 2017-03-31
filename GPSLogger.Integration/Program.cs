using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Extensions.Security;
using Common.Security;
using Common.Security.Signing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Common.Messages;
using GPSLogger.Models;
using Microsoft.AspNetCore.Hosting;

namespace GPSLogger.Integration
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ArrangeTypeModifiers
    class Program
    {
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            RunTests().Wait();
            Debug.WriteLine("All tests executed successfully");
        }

        private static async Task<TestServer> CreateServerAsync()
        {
            // Figure out where the GPS Logger directory is
            var startingLocation = new DirectoryInfo(Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location));
            var baseDirectory = Path.Combine(startingLocation.Parent.Parent.Parent.Parent.FullName, typeof(Startup).Namespace);

            // Create a temp directory for this server to run from
            var temp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " - " + Guid.NewGuid();
            var contentRoot = Path.Combine(baseDirectory, "tests", temp);
            var contentRootDirectory = Directory.CreateDirectory(contentRoot);

            // Assert that the newly created folder name matches what we expect
            var pattern = new Regex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}-[0-9]{2}-[0-9]{2} - [a-f0-9\-]+$");
            if (!pattern.IsMatch(contentRootDirectory.Name))
                throw new Exception();

            return await Task.Run(() =>
            {
                // Copy over needed files
                foreach (var file in new[]
                {
                    "appsettings.json"
                })
                    File.Copy(Path.Combine(baseDirectory, file), Path.Combine(contentRoot, file));

                // Spin up the server
                Debug.WriteLine($"Starting test server under {contentRoot}");
                return new TestServer(
                    new WebHostBuilder()
                        .UseContentRoot(contentRoot)
                        .UseStartup<Startup>()
                );
            });
        }

        private static async Task DoWithClientAsync(TestServer server, Func<HttpClient, Task> action)
        {
            using (var client = server.CreateClient())
            {
                await action(client);
            }
        }

        private static async Task<Credential<string>> GetCredentialAsync(TestServer server)
        {
            var result = default(Credential<string>);
            await DoWithClientAsync(server, async client =>
            {
                var response = await client.GetAsync("/api/credential");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(content);
                result = deserialized.Message.Contents;
            });
            return result;
        }

        private static async Task RunTests()
        {
            using (var server = await CreateServerAsync())
            {
                // Assert that no HMAC has been set yet
                await DoWithClientAsync(server, async client =>
                {
                    var response = await client.GetStringAsync("/api/hmackey");
                    var hmacIsSet = JsonConvert.DeserializeObject<bool>(response);
                    if (hmacIsSet)
                        throw new Exception("An HMAC key has already been set, even though we just started this server");
                });

                // Set an HMAC key
                await DoWithClientAsync(server, async client =>
                {
                    string newKey;
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        var buffer = new byte[100];
                        rng.GetBytes(buffer);
                        newKey = buffer.ToHexString();
                    }
                    var response = await client.PostAsync("/api/hmackey", new { newKey });
                    if (!string.IsNullOrWhiteSpace(response))
                        throw new Exception("The HMAC key controller responded with something after we told it to set the key");
                });

                // Assert that the HMAC has now been set
                await DoWithClientAsync(server, async client =>
                {
                    var response = await client.GetStringAsync("/api/hmackey");
                    var hmacIsSet = JsonConvert.DeserializeObject<bool>(response);
                    if (!hmacIsSet)
                        throw new Exception("An HMAC key has not been set, even though we just told the HMAC controller to set it");
                });

                // Get a credential
                var credential = await GetCredentialAsync(server);
                if (credential == null)
                    throw new Exception("Credential is null");
                if (string.IsNullOrWhiteSpace(credential.ID))
                    throw new Exception("Credential ID is null or whitespace");
                if (string.IsNullOrWhiteSpace(credential.Secret))
                    throw new Exception("Credential secret is null or whitespace");

                // Convert it into a credential of bytes
                var credentialBytes = await credential.ConvertAsync(async x => await ByteArrayExtensions.FromHexStringAsync(x));
                if (credentialBytes == null)
                    throw new Exception("Converted credential is null");
                if (credentialBytes.ID == null)
                    throw new Exception("Converted credential's ID is null");
                if (credentialBytes.Secret == null)
                    throw new Exception("Converted credential's secret is null");

                // Get another credential, signing the request with this credential
                var newCredentialResponse = default(SignedMessage<Credential<string>>);
                await DoWithClientAsync(server, async client =>
                {
                    var salt = BitConverter.GetBytes(DateTime.Now.Ticks);
                    var signedRequest = new SignedMessage<bool>
                    {
                        Message = new Message<bool>
                        {
                            Contents = true,
                            ID = credentialBytes.ID.ToHexString(),
                            Salt = salt.ToHexString(),
                            UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                        }
                    };
                    await SignAsync(signedRequest, credentialBytes, x => Task.Run(() => BitConverter.GetBytes(x)));

                    var queryString = $"/api/credential?contents={signedRequest.Message.Contents}&id={signedRequest.Message.ID}&hmac={signedRequest.HMAC}&salt={signedRequest.Message.Salt}&unixTime={signedRequest.Message.UnixTime}";
                    var response = await client.GetAsync(queryString);
                    response.EnsureSuccessStatusCode();
                    var contents = await response.Content.ReadAsStringAsync();
                    newCredentialResponse = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(contents);
                });
                if (newCredentialResponse == null)
                    throw new Exception("New credential was null");
                if (string.IsNullOrWhiteSpace(newCredentialResponse.HMAC))
                    throw new Exception("New credential's response wasn't signed");
                // Verify the new credential's response
                var hmacBefore = newCredentialResponse.HMAC;
                await SignAsync(newCredentialResponse, credentialBytes, async contents =>
                {
                    var id = await ByteArrayExtensions.FromHexStringAsync(contents.ID);
                    var secret = await ByteArrayExtensions.FromHexStringAsync(contents.Secret);
                    return new[]
                        {
                            id,
                            secret
                        }.SelectMany(_ => _)
                        .ToArray();
                });
                if (!hmacBefore.Equals(newCredentialResponse.HMAC, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception("The server didn't sign the response using our secret");

                // Get all the locations for the first credential
                var locations = default(IEnumerable<Location>);
                await DoWithClientAsync(server, async client =>
                {
                    var response = await client.GetStringAsync($"/api/location/?id={credential.ID}");
                    locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(response);
                });
                if (locations == null)
                    throw new Exception("Locations is null");
                if (locations.Any())
                    throw new Exception("Returned some locations, even though the credential is supposed to be new");

                // Post a location using the first credential
                var postResponse = default(SignedMessage<bool>);
                var postedLocation = new Location
                {
                    Latitude = -1000,
                    Longitude = 2000
                };
                await DoWithClientAsync(server, async client =>
                {
                    var request = new SignedMessage<Location>
                    {
                        Message = new Message<Location>
                        {
                            ID = credential.ID,
                            Salt = BitConverter.GetBytes(DateTimeOffset.Now.Ticks).ToHexString(),
                            UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            Contents = postedLocation
                        }
                    };
                    await SignAsync(
                        request,
                        credentialBytes,
                        location => Task.FromResult(new[] { location.Latitude, location.Longitude }.Select(BitConverter.GetBytes).SelectMany(_ => _).ToArray())
                        );
                    var response = await client.PostAsync($"/api/location", request);
                    postResponse = JsonConvert.DeserializeObject<SignedMessage<bool>>(response);
                });
                if (postResponse == null)
                    throw new Exception("Didn't receive a valid response after posting a location");
                if (string.IsNullOrWhiteSpace(postResponse.HMAC))
                    throw new Exception("The response from posting a location was not signed");
                if (postResponse.Message == null)
                    throw new Exception("The message within the signed response is null instead of having something");
                if (!postResponse.Message.Contents)
                    throw new Exception("The server indicated the post wasn't successful");

                // Get all the locations for the first credential again
                locations = default(IEnumerable<Location>);
                await DoWithClientAsync(server, async client =>
                {
                    var response = await client.GetStringAsync($"/api/location/?id={credential.ID}");
                    locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(response);
                });
                // Make sure we got back the location we just posted
                if (locations == null)
                    throw new Exception("Locations is null");
                var locationsArray = locations.ToArray();
                if (locationsArray.Length > 1)
                    throw new Exception("Got back more than one location, even though we only posted one");
                var firstLocation = locationsArray.FirstOrDefault();
                if (ReferenceEquals(firstLocation, null))
                    throw new Exception("Didn't return any locations, even though we just posted one");
                const double tolerance = 0.0000001;
                if (Math.Abs(firstLocation.Latitude - postedLocation.Latitude) > tolerance)
                    throw new Exception($"The returned latitude is more than {tolerance} off");
                if (Math.Abs(firstLocation.Longitude - postedLocation.Longitude) > tolerance)
                    throw new Exception($"The returned longitude is more than {tolerance} off");
            }
        }

        private static async Task SignAsync<T>(SignedMessage<T> signedRequest, Credential<byte[]> credential, Func<T, Task<byte[]>> conversion)
        {
            var serialized = new[]
                {
                    await conversion(signedRequest.Message.Contents),
                    await ByteArrayExtensions.FromHexStringAsync(signedRequest.Message.ID),
                    await ByteArrayExtensions.FromHexStringAsync(signedRequest.Message.Salt),
                    BitConverter.GetBytes(signedRequest.Message.UnixTime)
                }.SelectMany(_ => _)
                .ToArray();
            using (var hmac = new HMACMD5(credential.Secret))
            {
                var hash = hmac.ComputeHash(serialized);
                signedRequest.HMAC = hash.ToHexString();
            }
        }
    }
}