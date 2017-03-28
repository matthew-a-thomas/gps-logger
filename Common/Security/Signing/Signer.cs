using System.Threading.Tasks;
using Common.Extensions;
using Common.Messages;
using Common.Serialization;

namespace Common.Security.Signing
{
    /// <summary>
    /// Something that can sign things
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Signer<T>
    {
        /// <summary>
        /// Creates HMACs
        /// </summary>
        private readonly IHMACProvider _hmacProvider;
        private readonly ISerializer<Message<T>> _serializer;

        public Signer(
            IHMACProvider hmacProvider,
            ISerializer<Message<T>> serializer
            )
        {
            _hmacProvider = hmacProvider;
            _serializer = serializer;
        }
        
        /// <summary>
        /// Signs the given thing.
        /// Returns a copy of the thing with an HMAC field appended; the HMAC field has been populated with an HMAC using the given key
        /// </summary>
        /// <param name="thing">The thing to sign</param>
        /// <param name="hmacKey">The signing key</param>
        /// <returns></returns>
        public async Task<SignedMessage<T>> SignAsync(Message<T> thing, byte[] hmacKey)
        {
            var serialized = await _serializer.SerializeAsync(thing);
            using (var hmac = await _hmacProvider.GetAsync(hmacKey))
            {
                var signature = hmac.ComputeHash(serialized);
                var result = new SignedMessage<T>
                {
                    HMAC = signature.ToHexString(),
                    Message = thing
                };
                return result;
            }
        }
    }
}