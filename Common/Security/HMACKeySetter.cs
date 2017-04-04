using System;
using System.Threading.Tasks;
using Common.LocalStorage;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACKeySetter : IHMACKeySetter
    {
        private readonly string _keyName;
        private readonly IHMACKeyProvider _keyProvider;
        private readonly IKeySizeProvider _keySizeProvider;
        private readonly IStorage<byte[]> _storage;

        public HMACKeySetter(
            string keyName,
            IHMACKeyProvider keyProvider,
            IKeySizeProvider keySizeProvider,
            IStorage<byte[]> storage
            )
        {
            _keyName = keyName;
            _keyProvider = keyProvider;
            _keySizeProvider = keySizeProvider;
            _storage = storage;
        }

        public async Task SetAsync(byte[] newKey)
        {
            if(ReferenceEquals(newKey, null))
            throw new Exception("Please provide a new key");
            if (await _keyProvider.IsSetAsync())
                throw new Exception("The HMAC key has already been set");
            
            if (newKey.Length < _keySizeProvider.KeySize)
                throw new Exception($"Please provide at least {_keySizeProvider.KeySize} bytes");

            await _storage.SetAsync(_keyName, newKey);
        }
    }
}
