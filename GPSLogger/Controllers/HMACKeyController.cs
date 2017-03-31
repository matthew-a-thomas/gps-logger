using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.LocalStorage;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    /// <summary>
    /// Controller for the HMAC key, which is stored in persisted storage
    /// </summary>
    [Route("api/[controller]")]
    // ReSharper disable once InconsistentNaming
    public class HMACKeyController : ControllerBase
    {
        // ReSharper disable once ClassNeverInstantiated.Global
        public class PostParameters
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string NewKey { get; set; }
        }

        // ReSharper disable once InconsistentNaming
        private const string HMACKeyName = "hmac key";
        private const int MinKeySize = 16;

        private readonly IStorage<byte[]> _storage;

        public HMACKeyController(IStorage<byte[]> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Generates a new default key
        /// </summary>
        private static byte[] DefaultKeyGenerator() => Enumerable.Repeat((byte)0, MinKeySize).ToArray();

        /// <summary>
        /// Returns a boolean indicating whether the HMAC key has been set
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        // ReSharper disable once MemberCanBePrivate.Global
        public async Task<bool> GetAsync() => await _storage.ExistsAsync(HMACKeyName);
        
        /// <summary>
        /// Returns the current HMAC key.
        /// Do not expose this to clients
        /// </summary>
        /// <returns></returns>
        internal async Task<byte[]> GetCurrentAsync() => await _storage.GetAsync(HMACKeyName) ?? DefaultKeyGenerator();

        /// <summary>
        /// Sets the HMAC key if it hasn't already been set
        /// </summary>
        [HttpPost]
        // ReSharper disable once UnusedMember.Global
        public async Task PostAsync([FromBody] PostParameters parameters)
        {
            if (ReferenceEquals(parameters, null) || string.IsNullOrWhiteSpace(parameters.NewKey))
                throw new Exception("Please provide a new key");
            if (await GetAsync())
                throw new Exception("The HMAC key has already been set");
            
            var hmacKeyBytes = await ByteArrayExtensions.FromHexStringAsync(parameters.NewKey);
            if (hmacKeyBytes.Length < MinKeySize)
                throw new Exception("Please provide at least " + MinKeySize + " bytes");

            await _storage.SetAsync(HMACKeyName, hmacKeyBytes);
        }
    }
}
