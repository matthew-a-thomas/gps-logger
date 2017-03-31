using System;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Utilities;

namespace Common.LocalStorage
{
    /// <summary>
    /// Wraps an IStorage in a threadsafe way, adding a "GetOrAddAsync" method.
    /// Note that operations on the same key between different threads will be blocked; only one thread is allowed to perform an operation on a key at one time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentStorage<T> : IStorage<T>
    {
        private readonly IStorage<T> _backingStorage;
        private readonly Locker<string> _locker;

        public ConcurrentStorage(IStorage<T> backingStorage)
        {
            _backingStorage = backingStorage;
            _locker = new Locker<string>();
        }

        private bool Exists(string key) => GetWhileLocked(key, () => _backingStorage.ExistsAsync(key).WaitAndGet());
        public async Task<bool> ExistsAsync(string key) => await Task.Run(() => Exists(key));

        private T Get(string key) => GetWhileLocked(key, () => _backingStorage.GetAsync(key).WaitAndGet());
        public async Task<T> GetAsync(string key) => await Task.Run(() => Get(key));

        /// <summary>
        /// Returns the current value associated with the given key, using the given function to generate and assign the value if needed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public async Task<T> GetOrAddAsync(string key, Func<string, T> add)
        {
            return await Task.Run(() =>
            {
                var result = default(T);
                _locker.DoLocked(key, () =>
                {
                    if (!Exists(key))
                    {
                        Set(key, result = add(key));
                    }
                    else
                    {
                        result = Get(key);
                    }
                });
                return result;
            });
        }

        private TResult GetWhileLocked<TResult>(string key, Func<TResult> synchronousFn)
        {
            var result = default(TResult);
            _locker.DoLocked(key, () =>
            {
                result = synchronousFn();
            });
            return result;
        }

        private void Set(string key, T contents)
        {
            _locker.DoLocked(key, () =>
            {
                _backingStorage.SetAsync(key, contents).Wait();
            });
        }
        public async Task SetAsync(string key, T contents) => await Task.Run(() => Set(key, contents));
    }
}
