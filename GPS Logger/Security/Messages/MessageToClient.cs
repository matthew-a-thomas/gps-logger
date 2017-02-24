namespace GPS_Logger.Security.Messages
{
    /// <summary>
    /// A response message from the server to a client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageToClient<T>
    {
        /// <summary>
        /// The salt that the client sent up in its request
        /// </summary>
        public string ClientSalt { get; set; }

        /// <summary>
        /// The server's current time
        /// </summary>
        public long ServerEpoch { get; set; }

        /// <summary>
        /// A new salt from the server
        /// </summary>
        public string ServerSalt { get; set; }

        /// <summary>
        /// The server's response
        /// </summary>
        public T Contents { get; set; }

        /// <summary>
        /// Hashed using the client's secret, which the server is able to derive from the client's ID
        /// </summary>
        public string HMAC { get; set; }
    }
}