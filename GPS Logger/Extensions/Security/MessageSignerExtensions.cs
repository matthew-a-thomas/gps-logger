using GPS_Logger.Security;
using GPS_Logger.Security.Messages;
using GPS_Logger.Serialization;

namespace GPS_Logger.Extensions.Security
{
    public static class MessageSignerExtensions
    {
        /// <summary>
        /// Signs the given message using the given payload serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signer"></param>
        /// <param name="message"></param>
        /// <param name="hmacKey"></param>
        /// <param name="payloadSerializer"></param>
        public static void Sign<T>(this MessageSigner signer, MessageToClient<T> message, byte[] hmacKey, ISerializer<T> payloadSerializer) => signer.Sign(message, hmacKey, payloadSerializer.Serialize);

        /// <summary>
        /// Signs the given message
        /// </summary>
        /// <param name="signer"></param>
        /// <param name="message"></param>
        /// <param name="hmacKey"></param>
        public static void Sign(this MessageSigner signer, MessageToClient<bool> message, byte[] hmacKey) => signer.Sign(message, hmacKey, x => x);

        /// <summary>
        /// Signs the given message
        /// </summary>
        /// <param name="signer"></param>
        /// <param name="message"></param>
        /// <param name="hmacKey"></param>
        public static void Sign(this MessageSigner signer, MessageToClient<long> message, byte[] hmacKey) => signer.Sign(message, hmacKey, x => x);
    }
}