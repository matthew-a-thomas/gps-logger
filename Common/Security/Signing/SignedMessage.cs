using Common.Messages;

namespace Common.Security.Signing
{
    public class SignedMessage<T> : ISignable
    {
        public string HMAC { get; set; }
        public Message<T> Message { get; set; }
    }
}