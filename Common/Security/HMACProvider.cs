﻿using System.Security.Cryptography;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACProvider : IHMACProvider
    {
        private readonly Delegates.HMACKeyProvider _hmacKeyProvider;

        public HMACProvider(
            Delegates.HMACKeyProvider hmacKeyProvider
            )
        {
            _hmacKeyProvider = hmacKeyProvider;
        }

        public HMAC Get() => Get(_hmacKeyProvider());

        public HMAC Get(byte[] key) => new HMACMD5(key);
    }
}