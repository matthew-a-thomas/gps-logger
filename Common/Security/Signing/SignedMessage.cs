using System.Collections.Generic;
using Common.Messages;
using System.Linq;

namespace Common.Security.Signing
{
    public class SignedMessage<T> : Message<T>, ISignable
    {
        public string HMAC { get; set; }
    }
}