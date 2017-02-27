using System.Security.Cryptography;

namespace GPS_Logger.Security
{
    /// <summary>
    /// Provides an HMAC
    /// </summary>
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