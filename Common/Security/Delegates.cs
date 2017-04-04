using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Common.Security
{
    public static class Delegates
    {
        /// <summary>
        /// Delegate that generates a Credential for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate Task<Credential<byte[]>> GenerateCredentialDelegateAsync(byte[] id);

        /// <summary>
        /// Delegate that generates a random salt
        /// </summary>
        /// <returns></returns>
        public delegate Task<byte[]> GenerateSaltDelegateAsync();
        
        /// <summary>
        /// Delegate that generates a new RandomNumberGenerator.
        /// The returned object is disposed after use
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public delegate Task<RandomNumberGenerator> RNGFactoryAsync();
    }
}