using System.IO;
using System.Threading.Tasks;

namespace Common.LocalStorage
{
    public interface IPersistentStore
    {
        Task<bool> ExistsAsync(string key);

        Task<Stream> OpenAsync(string key, Options options);
    }
}