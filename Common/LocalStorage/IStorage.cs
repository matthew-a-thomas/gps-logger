using System.Threading.Tasks;

namespace Common.LocalStorage
{
    public interface IStorage
    {
        /// <summary>
        /// Indicates if the given key exists in this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ValueTask<bool> ExistsAsync(string key);

        /// <summary>
        /// Retrieves all the contents of the given key from this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ValueTask<byte[]> GetAsync(string key);

        /// <summary>
        /// Overwrites all the contents of the given key with the given contents through this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        Task SetAsync(string key, byte[] contents);
    }
}