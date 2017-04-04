using System.Threading.Tasks;
using Common.LocalStorage;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACKeyProvider : IHMACKeyProvider
    {
        private static readonly byte[] DefaultKey = new byte[0];

        private readonly string _keyName;
        private readonly IStorage<byte[]> _storage;
        
        public HMACKeyProvider(
            string keyName,
            IStorage<byte[]> storage
            )
        {
            _keyName = keyName;
            _storage = storage;
        }

        public async Task<byte[]> GetCurrentAsync() => await _storage.GetAsync(_keyName) ?? DefaultKey;

        public async Task<bool> IsSetAsync() => await _storage.ExistsAsync(_keyName);
    }
}
