using Common.Messages;

namespace Common.Security.Signing
{
    public class SignedMessage<T> : Message<T>, ISignable
    {
        public string HMAC { get; set; }
    }
}