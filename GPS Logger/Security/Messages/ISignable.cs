namespace GPS_Logger.Security.Messages
{
    public interface ISignable
    {
        // ReSharper disable once InconsistentNaming
        byte[] HMAC { get; set; }
    }
}