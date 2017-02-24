using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GPS_Logger.LocalStorage
{
    /// <summary>
    /// Wraps an IPersistentStore to give easier read/write access to keys
    /// </summary>
    public class PersistentStoreManager
    {
        private readonly IPersistentStore _persistentStore;
        private readonly ConditionalWeakTable<string, object> _keyLocks;

        public PersistentStoreManager(IPersistentStore persistentStore)
        {
            _persistentStore = persistentStore;
            _keyLocks = new ConditionalWeakTable<string, object>();
        }

        /// <summary>
        /// Executes an action within a lock that is associated with the given key.
        /// Note that the key is case insensitive
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        private void DoLocked(string key, Action action)
        {
            lock (_keyLocks.GetValue(key.ToLower(), x => new object())) // .ToLower makes this case insensitive
                action();
        }

        /// <summary>
        /// Returns the data associated with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] Get(string key)
        {
            byte[] buffer = null;
            DoLocked(key, () =>
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
            DoLocked(key, () =>
            {
                using (var stream = _persistentStore.Open(key, new Options { FileAccess = FileAccess.Write, FileMode = FileMode.Create, FileShare = FileShare.None }))
                    stream.Write(value, 0, value.Length);
            });
        }
    }
}