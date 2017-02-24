using System.Security.Cryptography;

namespace GPS_Logger.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static byte[] GetBytes(this RandomNumberGenerator rng, int numBytes)
        {
            var buffer = new byte[numBytes];
            rng.GetBytes(buffer);
            return buffer;
        }
    }
}