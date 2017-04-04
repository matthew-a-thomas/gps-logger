using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class KeyedHMACProvider : IKeyedHMACProvider
    {
        private readonly IHMACKey _key;
        private readonly IHMACProvider _hmacProvider;

        public KeyedHMACProvider(
            IHMACKey key,
            IHMACProvider hmacProvider
            )
        {
            _key = key;
            _hmacProvider = hmacProvider;
        }

        public async Task<HMAC> GetAsync() => await _hmacProvider.GetAsync(await _key.GetCurrentAsync());
    }
}