using System.Threading.Tasks;

namespace Common.LocalStorage
{
    public interface IStorage<T>
    {
        /// <summary>
        /// Indicates if the given key exists in this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Retrieves all the contents of the given key from this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetAsync(string key);

        /// <summary>
        /// Overwrites all the contents of the given key with the given contents through this persistent store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        Task SetAsync(string key, T contents);
    }
}