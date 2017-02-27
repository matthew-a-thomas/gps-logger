namespace GPS_Logger.Security.Messages
{
    public interface ISignable
    {
        byte[] HMAC { get; set; }
    }
}