using System.Security.Cryptography;
using GPS_Logger.Models;

namespace GPS_Logger.Security
{
    public static class Delegates
    {
        /// <summary>
        /// Delegate that generates a Credential for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate Credential GenerateCredentialDelegate(byte[] id);

        /// <summary>
        /// Delegate that generates a random salt
        /// </summary>
        /// <returns></returns>
        public delegate byte[] GenerateSaltDelegate();

        /// <summary>
        /// Delegate that generates a new HMAC.
        /// The returned object needs to be disposed after use
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public delegate HMAC HMACFactory();

        /// <summary>
        /// Delegate that generates a new RandomNumberGenerator.
        /// The returned object is disposed after use
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public delegate RandomNumberGenerator RNGFactory();
    }
}