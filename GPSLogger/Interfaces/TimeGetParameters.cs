using System.Diagnostics.CodeAnalysis;

namespace GPSLogger.Interfaces
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TimeGetParameters
    {
        public bool Contents { get; set; }
        public string HMAC { get; set; }
        public string ID { get; set; }
        public string Salt { get; set; }
        public long UnixTime { get; set; }
    }
}
