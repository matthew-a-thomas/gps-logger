using System;
using System.Collections.Concurrent;
using System.Linq;
using GPS_Logger.Extensions;
using GPS_Logger.Serialization;

namespace GPS_Logger.Security.Messages.Signing
{
    public class MessageValidator
    {
        private readonly Delegates.HMACFactory _hmacFactory;
        private readonly Delegates.GenerateCredentialDelegate _generateCredentials;
        private readonly ConcurrentDictionary<Type, object> _serializers;

        public MessageValidator(
            Delegates.HMACFactory hmacFactory,
            Delegates.GenerateCredentialDelegate generateCredentials)
        {
            _hmacFactory = hmacFactory;
            _generateCredentials = generateCredentials;
            _serializers = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Determines if the given message from a client is valid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="message"></param>
        /// <param name="forCredential"></param>
        /// <param name="payloadSerializationStep"></param>
        /// <returns></returns>
        public bool IsValid<T, U>(MessageFromClient<T> message, Func<T, U> payloadSerializationStep)
        {
            try
            {
                if (ReferenceEquals(message, null))
                    return false; // The message itself is null

                // Make sure it's a full (non-null) message
                if (new object[] { message.ClientSalt, message.Contents, message.HMAC, message.ID }.Any(x => ReferenceEquals(x, null)))
                    return false; // One of the properties is null

                // See if the message's time is too different than now
                var reportedTimeDifference = DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(message.ClientEpoch);
                if (Math.Abs(reportedTimeDifference.TotalMinutes) > 1)
                    return false; // The reported time is too far off from now

                // Make sure the hex fields have valid hex values
                if (new[] { message.ClientSalt, message.HMAC, message.ID }.Any(x =>
               {
                   try
                   {
                       ByteArrayExtensions.FromHexString(x);
                       return true;
                   }
                   catch
                   {
                       return false;
                   }
               }))
                    return false;

                // Look up a serializer that has already been made for this type, or create a new one
                var serializer =
                    (Serializer<MessageFromClient<T>>)
                    _serializers
                    .GetOrAdd(typeof(T), t =>
                    {
                        var newSerializer = new Serializer<MessageFromClient<T>>();

                        newSerializer.EnqueueStep(x => x.ClientEpoch);
                        newSerializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.ID));
                        newSerializer.EnqueueStep(x => payloadSerializationStep(x.Contents));
                        newSerializer.EnqueueStep(x => ByteArrayExtensions.FromHexString(x.ClientSalt));

                        return newSerializer;
                    });

                // Serialize the message
                var serialized = serializer.Serialize(message);

                // Derive the client's credentials from the ID they gave in the message
                var purportedCredential = _generateCredentials(ByteArrayExtensions.FromHexString(message.ID));

                // Now see if the HMAC (using the given credential's secret) matches the reported HMAC
                using (var hmac = _hmacFactory())
                {
                    hmac.Key = ByteArrayExtensions.FromHexString(purportedCredential.Secret); // Set the HMAC key to their secret
                    var hash = hmac.ComputeHash(serialized);
                    var hashString = hash.ToHexString();

                    return hashString.Equals(message.HMAC, StringComparison.InvariantCultureIgnoreCase); // Compare the reported hash with the computed hash
                }
            }
            catch
            {
                return false; // Something went wrong
            }
        }
    }
}