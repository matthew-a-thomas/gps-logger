using System;
using System.Collections.Generic;
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
using Common.Messages;
using GPSLogger.Models;

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

        public Program()
        {
            _server = Helpers.CreateServerAsync().WaitAndGet();
        }

        private readonly TestServer _server;

        private async Task DoWithClientAsync(Func<HttpClient, Task> action)
        {
            using (var client = _server.CreateClient())
            {
                await action(client);
            }
        }

        private async Task<Credential<string>> GetCredentialAsync()
        {
            var result = default(Credential<string>);
            await DoWithClientAsync(async client =>
            {
                var response = await client.GetAsync("/api/credential");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<SignedMessage<Credential<string>>>(content);
                result = deserialized.Message.Contents;
            });
            return result;
        }

        private async Task RunTests()
        {
            // Get a credential
            var credential = await GetCredentialAsync();
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
            await DoWithClientAsync(async client =>
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

                var response = await client.GetAsync(
                    $"/api/credential?contents={signedRequest.Message.Contents}&id={signedRequest.Message.ID}&hmac={signedRequest.HMAC}&salt={signedRequest.Message.Salt}&unixTime={signedRequest.Message.UnixTime}");
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
            await DoWithClientAsync(async client =>
            {
                var response = await client.GetStringAsync($"/api/location/?id={credential.ID}");
                locations = JsonConvert.DeserializeObject<IEnumerable<Location>>(response);
            });
            if (locations == null)
                throw new Exception("Locations is null");
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