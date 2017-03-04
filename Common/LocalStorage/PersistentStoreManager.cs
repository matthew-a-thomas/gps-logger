using System;
using System.IO;
using System.Runtime.CompilerServices;
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
        public byte[] Get(string key)
        {
            byte[] buffer = null;
            _locker.DoLocked(key, () =>
            {
                if (!IsSet(key))
                    return; // We won't be able to open the stream for reading
                using (var stream = _persistentStore.Open(key, new Options { FileAccess = FileAccess.Read, FileMode = FileMode.Open, FileShare = FileShare.Read }))
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                }
            });
            return buffer;
        }

        /// <summary>
        /// Determines whether the given key has been set yet
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsSet(string key) => _persistentStore.Exists(key);

        /// <summary>
        /// Assigns the value to the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, byte[] value)
        {
            if (key == null || value == null) return;
            _locker.DoLocked(key, () =>
            {
                using (var stream = _persistentStore.Open(key, new Options { FileAccess = FileAccess.Write, FileMode = FileMode.Create, FileShare = FileShare.None }))
                    stream.Write(value, 0, value.Length);
            });
        }
    }
}