using GPS_Logger.Security.Messages;
using GPS_Logger.Security.Messages.Signing;
using GPS_Logger.Serialization;

namespace GPS_Logger.Extensions.Security
{
    public static class MessageValidatorExtensions
    {
        /// <summary>
        /// Determines if the given message is valid by using the given payload serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validator"></param>
        /// <param name="message"></param>
        /// <param name="payloadSerializer"></param>
        /// <returns></returns>
        public static bool IsValid<T>(this MessageValidator validator, MessageFromClient<T> message, ISerializer<T> payloadSerializer) => validator.IsValid(message, payloadSerializer.Serialize);

        /// <summary>
        /// Determines if the given message is valid
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsValid(this MessageValidator validator, MessageFromClient<bool> message) => validator.IsValid(message, x => x);
    }
}