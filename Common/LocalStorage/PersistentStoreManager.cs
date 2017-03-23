using System.IO;
using System.Threading.Tasks;
using Common.Utilities;

namespace Common.LocalStorage
{
    /// <summary>
    /// Wraps an IPersistentStore to give easier read/write access to keys
    /// </summary>
    public class PersistentStoreManager
    {
        private readonly IPersistentStore _persistentStore;
        private readonly Locker<string> _locker;
        
        public PersistentStoreManager(IPersistentStore persistentStore)
        {
            _persistentStore = persistentStore;
            _locker = new Locker<string>();
        }
        
        /// <summary>
        /// Returns the data associated with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<byte[]> GetAsync(string key)
        {
            if (key == null) return null;
            key = await FileSystemPathSanitizer.SanitizeAsync(key);
            byte[] buffer = null;
            _locker.DoLockedAsync(key, () =>
            {
                Task.Run(async () =>
                {
                    if (!await IsSetInternalAsync(key))
                        return; // We won't be able to open the stream for reading
                    using (var stream = await _persistentStore.OpenAsync(key, new Options { FileAccess = FileAccess.Read, FileMode = FileMode.Open, FileShare = FileShare.Read }))
                    {
                        buffer = new byte[stream.Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                }).Wait();
            });
            return buffer;
        }

        /// <summary>
        /// Determines whether the given key has been set yet
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> IsSetAsync(string key)
        {
            key = await FileSystemPathSanitizer.SanitizeAsync(key);
            return await IsSetInternalAsync(key);
        }

        /// <summary>
        /// Same as calling _persistentStore.Exists
        /// </summary>
        /// <param name="sanitizedKey"></param>
        /// <returns></returns>
        private async Task<bool> IsSetInternalAsync(string sanitizedKey) => await _persistentStore.ExistsAsync(sanitizedKey);
        
        /// <summary>
        /// Assigns the value to the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task SetAsync(string key, byte[] value)
        {
            if (key == null || value == null) return;
            key = await FileSystemPathSanitizer.SanitizeAsync(key);
            _locker.DoLockedAsync(key.ToLower(), async () =>
                {
                    using (var stream = await _persistentStore.OpenAsync(key, new Options { FileAccess = FileAccess.Write, FileMode = FileMode.Create, FileShare = FileShare.None }))
                        await stream.WriteAsync(value, 0, value.Length);
                });
        }
    }
}