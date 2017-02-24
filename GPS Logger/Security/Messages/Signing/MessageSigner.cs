using System;
using System.Collections.Concurrent;
using GPS_Logger.Extensions;
using GPS_Logger.Security.Messages;
using GPS_Logger.Serialization;

namespace GPS_Logger.Security
{
    /// <summary>
    /// Handles HMAC'ing instances of MessageToClient
    /// </summary>
    public class MessageSigner
    {
        private readonly Delegates.HMACFactory _hmacFactory;
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>> _serializers;

        public MessageSigner(Delegates.HMACFactory hmacFactory)
        {
            _hmacFactory = hmacFactory;
            _serializers = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>>();
        }
        
        /// <summary>
        /// Signs the given message using a function that can translate the payload into a simple type
        /// </summary>
        /// <typeparam name="T">The payload type within the message</typeparam>
        /// <typeparam name="U">The type that the payload can be converted into, which the default serializer can already handle</typeparam>
        /// <param name="message">The message to sign</param>
        /// <param name="hmacKey">The key to use when signing</param>
        /// <param name="payloadSerializationStep">A function that can translate the message's payload into something the serializer can easily handle</param>
        public void Sign<T, U>(MessageToClient<T> message, byte[] hmacKey, Func<T, U> payloadSerializationStep)
        {
            // Lookup a serializer that has already been created for the type parameters, or create our own
            var serializer =
                (Serializer<MessageToClient<T>>)
                _serializers
                .GetOrAdd(typeof(T), t => new ConcurrentDictionary<Type, object>()) // Find the dictionary of serializers that goes with T
                .GetOrAdd(typeof(U), t => // Find the serializer that translates payloads from T to U
                {
                    var newSerializer = new Serializer<MessageToClient<T>>();

                    newSerializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.ClientSalt));
                    newSerializer.EnqueueStep(x => payloadSerializationStep(x.Contents));
                    newSerializer.EnqueueStep(x => x.ServerEpoch);
                    newSerializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.ServerSalt));

                    return newSerializer;
                });
            
            // Compute the hash, and put the computed hash into the message
            using (var hmac = _hmacFactory())
            {
                hmac.Key = hmacKey;
                var hash = hmac.ComputeHash(serializer.Serialize(message));
                message.HMAC = hash.ToHexString();
            }
        }
    }
}