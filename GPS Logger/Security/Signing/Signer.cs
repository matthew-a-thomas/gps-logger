using AutoMapper;
using GPS_Logger.Extensions;
using GPS_Logger.Serialization;

namespace GPS_Logger.Security.Signing
{
    /// <summary>
    /// Something that can sign things
    /// </summary>
    public class Signer<TSigned, TUnsigned>
        where TSigned : TUnsigned, ISignable, new()
    {
        /// <summary>
        /// Creates HMACs
        /// </summary>
        private readonly IHMACProvider _hmacProvider;
        private readonly ISerializer<TUnsigned> _serializer;

        public Signer(
            IHMACProvider hmacProvider,
            ISerializer<TUnsigned> serializer
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
        public TSigned Sign(TUnsigned thing, byte[] hmacKey)
        {
            var result = Mapper.Map<TSigned>(thing);
            var serialized = _serializer.Serialize(thing);
            using (var hmac = _hmacProvider.Get(hmacKey))
            {
                var signature = hmac.ComputeHash(serialized);
                result.HMAC = signature.ToHexString();
            }
            return result;
        }
    }
}