using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    /// <summary>
    /// Provides an HMAC
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IHMACProvider
    {
        /// <summary>
        /// Create an HMAC with the default key
        /// </summary>
        /// <returns></returns>
        Task<HMAC> GetAsync();

        /// <summary>
        /// Create an HMAC with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<HMAC> GetAsync(byte[] key);
    }
}