using System.Threading.Tasks;

namespace Common.Security
{
    public interface IHMACKeySetter
    {
        Task SetAsync(byte[] newKey);
    }
}
