using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACProvider : IHMACProvider
    {
        private readonly IHMACKey _key;

        public HMACProvider(
            IHMACKey key
            )
        {
            _key = key;
        }

        public async Task<HMAC> GetAsync() => await GetAsync(await _key.GetCurrentAsync());

        public Task<HMAC> GetAsync(byte[] key) => Task.FromResult<HMAC>(new HMACSHA256(key));
    }
}