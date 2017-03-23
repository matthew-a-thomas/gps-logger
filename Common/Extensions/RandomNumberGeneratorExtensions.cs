using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static Task<byte[]> GetBytesAsync(this RandomNumberGenerator rng, int numBytes) => Task.Run(() => GetBytes(rng, numBytes));

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