using System.Threading.Tasks;
using Common.Messages;

namespace Common.Serialization
{
    /// <summary>
    /// Serializes messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageSerializer<T> : ISerializer<Message<T>>
    {
        /// <summary>
        /// The serializer that does all the work
        /// </summary>
        private readonly ISerializer<Message<T>> _serializer;

        /// <summary>
        /// Creates a new serializer that can serialize messages using the given contents serializer
        /// </summary>
        /// <param name="contentsSerializer"></param>
        public MessageSerializer(
            ISerializer<T> contentsSerializer
            )
        {
            // Set up a new serializer to handle messages
            var serializer = new Serializer<Message<T>>();

            serializer.EnqueueStep(x => await contentsSerializer.SerializeAsync(x.Contents)); // Delegate serialization of the contents to the provided contentsSerializer
            serializer.EnqueueStep(x => x.ID);
            serializer.EnqueueStep(x => x.Salt);
            serializer.EnqueueStep(x => x.UnixTime);

            _serializer = serializer;
        }

        /// <summary>
        /// Serializes the given message
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public async Task<byte[]> SerializeAsync(Message<T> thing) => await _serializer.SerializeAsync(thing);
    }
}