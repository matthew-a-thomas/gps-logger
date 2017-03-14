using System;
using System.Linq;
using Common.Extensions;
using Common.LocalStorage;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    /// <summary>
    /// Controller for the HMAC key, which is stored in persisted storage
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [Route("api/[controller]")]
    public class HMACKeyController : ControllerBase
    {
        public class PostParameters
        {
            public string NewKey { get; set; }
        }

        // ReSharper disable once InconsistentNaming
        private const string HMACKeyName = "hmac key";
        private const int MinKeySize = 16;

        private readonly PersistentStoreManager _persistentStoreManager;

        public HMACKeyController(PersistentStoreManager persistentStoreManager)
        {
            _persistentStoreManager = persistentStoreManager;
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
        public bool Get() => _persistentStoreManager.IsSet(HMACKeyName);
        
        /// <summary>
        /// Returns the current HMAC key.
        /// Do not expose this to clients
        /// </summary>
        /// <returns></returns>
        internal byte[] GetCurrent() => _persistentStoreManager.Get(HMACKeyName) ?? DefaultKeyGenerator();

        /// <summary>
        /// Sets the HMAC key if it hasn't already been set
        /// </summary>
        /// <param name="newKey"></param>
        [HttpPost]
        public void Post([FromBody] PostParameters parameters)
        {
            if (ReferenceEquals(parameters, null) || string.IsNullOrWhiteSpace(parameters.NewKey))
                throw new Exception("Please provide a new key");
            if (Get())
                throw new Exception("The HMAC key has already been set");
            
            var hmacKeyBytes = ByteArrayExtensions.FromHexString(parameters.NewKey);
            if (hmacKeyBytes.Length < MinKeySize)
                throw new Exception("Please provide at least " + MinKeySize + " bytes");

            _persistentStoreManager.Set(HMACKeyName, hmacKeyBytes);
        }
    }
}
