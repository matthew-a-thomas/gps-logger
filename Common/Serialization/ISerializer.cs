using System.Threading.Tasks;

namespace Common.Serialization
{
    public interface ISerializer<in T>
    {
        Task<byte[]> SerializeAsync(T thing);
    }
}