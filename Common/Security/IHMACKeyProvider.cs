using System.Threading.Tasks;

namespace Common.Security
{
    public interface IHMACKeyProvider
    {
        Task<byte[]> GetCurrentAsync();
        Task<bool> IsSetAsync();
    }
}
