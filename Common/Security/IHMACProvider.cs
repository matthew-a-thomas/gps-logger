using System.Security.Cryptography;

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
        HMAC Get();

        /// <summary>
        /// Create an HMAC with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        HMAC Get(byte[] key);
    }
}