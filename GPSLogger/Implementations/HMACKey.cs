using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.LocalStorage;
using Common.Security;
using GPSLogger.Interfaces;

namespace GPSLogger.Implementations
{
    // ReSharper disable once InconsistentNaming
    public class HMACKey : IHMACKey
    {
        private readonly IKeySizeProvider _keySizeProvider;
        private readonly IStorage<byte[]> _storage;

        // ReSharper disable once InconsistentNaming
        private const string HMACKeyName = "hmac key";

        public HMACKey(
            IKeySizeProvider keySizeProvider,
            IStorage<byte[]> storage
            )
        {
            _keySizeProvider = keySizeProvider;
            _storage = storage;
        }

        /// <summary>
        /// Generates a new default key
        /// </summary>
        private byte[] DefaultKeyGenerator() => Enumerable.Repeat((byte)0, _keySizeProvider.KeySize).ToArray();

        /// <summary>
        /// Returns the current HMAC key.
        /// Do not expose this to clients
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GetCurrentAsync() => await _storage.GetAsync(HMACKeyName) ?? DefaultKeyGenerator();

        public async Task<bool> IsSetAsync() => await _storage.ExistsAsync(HMACKeyName);

        public async Task SetAsync(HMACPostParameters parameters)
        {
            if (ReferenceEquals(parameters, null) || string.IsNullOrWhiteSpace(parameters.NewKey))
                throw new Exception("Please provide a new key");
            if (await IsSetAsync())
                throw new Exception("The HMAC key has already been set");

            var hmacKeyBytes = await ByteArrayExtensions.FromHexStringAsync(parameters.NewKey);
            if (hmacKeyBytes.Length < _keySizeProvider.KeySize)
                throw new Exception($"Please provide at least {_keySizeProvider.KeySize} bytes");

            await _storage.SetAsync(HMACKeyName, hmacKeyBytes);
        }
    }
}
