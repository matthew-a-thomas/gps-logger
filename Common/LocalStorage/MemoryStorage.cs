using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Common.LocalStorage
{
    /// <summary>
    /// In-memory implementation of storage
    /// </summary>
    public class MemoryStorage : IStorage
    {
        private ConcurrentDictionary<string, byte[]> _memory = new ConcurrentDictionary<string, byte[]>();

        public ValueTask<bool> ExistsAsync(string key) => new ValueTask<bool>(_memory.ContainsKey(key));

        public ValueTask<byte[]> GetAsync(string key) => _memory.TryGetValue(key, out byte[] value) ? new ValueTask<byte[]>(value) : new ValueTask<byte[]>((byte[])null);

        public Task SetAsync(string key, byte[] contents)
        {
            _memory.AddOrUpdate(key, _ => contents,
                (_, __) => contents);
            return Task.CompletedTask;
        }
    }
}
