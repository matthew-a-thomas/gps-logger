using GPS_Logger.Models.Messages;

namespace GPS_Logger.Security.Signing
{
    public class SignedMessage<T> : Message<T>, ISignable
    {
        public byte[] HMAC { get; set; }
    }
}