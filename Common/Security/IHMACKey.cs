using System.Threading.Tasks;

namespace Common.Security
{
    // ReSharper disable once InconsistentNaming
    public interface IHMACKey
    {
        Task<byte[]> GetCurrentAsync();
        Task<bool> IsSetAsync();
        Task SetAsync(byte[] newKey);
    }
}
