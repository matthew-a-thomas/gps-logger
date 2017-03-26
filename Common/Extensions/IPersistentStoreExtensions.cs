using System.IO;
using System.Threading.Tasks;
using Common.LocalStorage;

namespace Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IPersistentStoreExtensions
    {
        /// <summary>
        /// Retrieves all the contents of the given key from this persistent store
        /// </summary>
        /// <param name="persistentStore"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async ValueTask<byte[]> GetAsync(this IPersistentStore persistentStore, string key)
        {
            if (key == null) return null;
            if (!await persistentStore.ExistsAsync(key))
                return null; // We won't be able to open the stream for reading because the item doesn't exist
            using (var stream = await persistentStore.OpenAsync(key, new Options { FileAccess = FileAccess.Read, FileMode = FileMode.Open, FileShare = FileShare.Read }))
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Overwrites all the contents of the given key with the given contents through this persistent store
        /// </summary>
        /// <param name="persistentStore"></param>
        /// <param name="key"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static async Task SetAsync(this IPersistentStore persistentStore, string key, byte[] contents)
        {
            if (key == null || contents == null) return;
            using (var stream = await persistentStore.OpenAsync(key, new Options { FileAccess = FileAccess.Write, FileMode = FileMode.Create, FileShare = FileShare.None }))
                await stream.WriteAsync(contents, 0, contents.Length);
        }
    }
}
