using System.Threading.Tasks;

namespace GPSLogger.Interfaces
{
    // ReSharper disable once InconsistentNaming
    public interface IHMACKey
    {
        Task<byte[]> GetCurrentAsync();
        Task<bool> IsSetAsync();
        Task SetAsync(HMACPostParameters parameters);
    }
}
