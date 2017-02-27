namespace GPS_Logger.Security.Messages
{
    public class SignedMessage<T> : Message<T>, ISignable
    {
        public byte[] HMAC { get; set; }
    }
}