﻿using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Common.LocalStorage
{
    /// <summary>
    /// In-memory implementation of storage
    /// </summary>
    public class MemoryStorage<T> : IStorage<T>
    {
        private readonly ConcurrentDictionary<string, T> _memory = new ConcurrentDictionary<string, T>();

        public Task<bool> ExistsAsync(string key) => Task.FromResult(_memory.ContainsKey(key));

        public Task<T> GetAsync(string key) => _memory.TryGetValue(key, out T value) ? Task.FromResult(value) : Task.FromResult(default(T));

        public Task SetAsync(string key, T contents)
        {
            _memory.AddOrUpdate(key, _ => contents,
                (_, __) => contents);
            return Task.CompletedTask;
        }
    }
}
