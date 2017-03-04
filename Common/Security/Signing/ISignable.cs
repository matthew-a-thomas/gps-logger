namespace Common.Security.Signing
{
    public interface ISignable
    {
        // ReSharper disable once InconsistentNaming
        string HMAC { get; set; }
    }
}