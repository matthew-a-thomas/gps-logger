using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACProvider : IHMACProvider
    {
        private readonly Delegates.HMACKeyProviderAsync _hmacKeyProviderAsync;

        public HMACProvider(
            Delegates.HMACKeyProviderAsync hmacKeyProviderAsync
            )
        {
            _hmacKeyProviderAsync = hmacKeyProviderAsync;
        }

        public async Task<HMAC> GetAsync() => await GetAsync(await _hmacKeyProviderAsync());

        public async Task<HMAC> GetAsync(byte[] key) => await Task.FromResult((HMAC)new HMACMD5(key));
    }
}