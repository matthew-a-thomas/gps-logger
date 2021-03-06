﻿using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public class HMACProvider : IHMACProvider
    {
        public Task<HMAC> GetAsync(byte[] key) => Task.FromResult<HMAC>(new HMACSHA256(key));
    }
}
