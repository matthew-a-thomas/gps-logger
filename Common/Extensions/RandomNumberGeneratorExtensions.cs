using System.Security.Cryptography;

namespace Common.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static byte[] GetBytes(this RandomNumberGenerator rng, int numBytes)
        {
            if (numBytes < 0)
                return null;
            var buffer = new byte[numBytes];
            rng.GetBytes(buffer);
            return buffer;
        }
    }
}