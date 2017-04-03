using System.Diagnostics.CodeAnalysis;

namespace GPSLogger.Interfaces
{
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HMACPostParameters
    {
        public string NewKey { get; set; }
    }
}
