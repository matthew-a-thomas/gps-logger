using System;
using System.Threading.Tasks;
using Common.LocalStorage;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACKey : IHMACKey
    {
        private static readonly byte[] DefaultKey = new byte[0];

        private readonly string _keyName;
        private readonly IStorage<byte[]> _storage;
        private readonly IKeySizeProvider _keySizeProvider;

        public HMACKey(
            string keyName,
            IStorage<byte[]> storage,
            IKeySizeProvider keySizeProvider
            )
        {
            _keyName = keyName;
            _storage = storage;
            _keySizeProvider = keySizeProvider;
        }

        public async Task<byte[]> GetCurrentAsync() => await _storage.GetAsync(_keyName) ?? DefaultKey;

        public async Task<bool> IsSetAsync() => await _storage.ExistsAsync(_keyName);

        public async Task SetAsync(byte[] newKey)
        {
            if (ReferenceEquals(newKey, null))
                throw new Exception("Please provide a new key");
            if (await IsSetAsync())
                throw new Exception("The HMAC key has already been set");

            if (newKey.Length < _keySizeProvider.KeySize)
                throw new Exception($"Please provide at least {_keySizeProvider.KeySize} bytes");

            await _storage.SetAsync(_keyName, newKey);
        }
    }
}
