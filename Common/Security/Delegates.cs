﻿using System.Security.Cryptography;

namespace Common.Security
{
    public static class Delegates
    {
        /// <summary>
        /// Delegate that generates a Credential for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate Credential<byte[]> GenerateCredentialDelegate(byte[] id);

        /// <summary>
        /// Delegate that generates a random salt
        /// </summary>
        /// <returns></returns>
        public delegate byte[] GenerateSaltDelegate();
        
        /// <summary>
        /// Returns a copy of this server's HMAC key
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public delegate byte[] HMACKeyProvider();
        
        /// <summary>
        /// Delegate that generates a new RandomNumberGenerator.
        /// The returned object is disposed after use
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public delegate RandomNumberGenerator RNGFactory();
    }
}