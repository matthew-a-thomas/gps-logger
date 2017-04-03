using System.Diagnostics.CodeAnalysis;

namespace GPSLogger.Interfaces
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CredentialGetParameters
    {
        public bool Contents { get; set; }
        public string HMAC { get; set; }
        public string ID { get; set; }
        public string Salt { get; set; }
        public long UnixTime { get; set; }
    }
}
