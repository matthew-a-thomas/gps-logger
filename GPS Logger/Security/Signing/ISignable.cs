namespace GPS_Logger.Security.Signing
{
    public interface ISignable
    {
        // ReSharper disable once InconsistentNaming
        byte[] HMAC { get; set; }
    }
}