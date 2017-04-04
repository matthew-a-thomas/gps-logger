using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public interface IKeyedHMACProvider
    {
        Task<HMAC> GetAsync();
    }
}
