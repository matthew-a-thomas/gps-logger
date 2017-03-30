using Common.Utilities;
using System.IO;
using System.Threading.Tasks;

namespace Common.LocalStorage
{
    /// <summary>
    /// Provides an IStorage interface to physical files.
    /// Note that this class should be wrapped in a ConcurrentStorage class if you'd like thread safety
    /// </summary>
    public class PhysicalStorage : IStorage<byte[]>
    {
        private readonly DirectoryInfo _storageDirectory;
        private readonly int _maxKeyLength;

        public PhysicalStorage(DirectoryInfo storageDirectory, int maxKeyLength)
        {
            _storageDirectory = storageDirectory;
            _maxKeyLength = maxKeyLength;
        }

        public async ValueTask<bool> ExistsAsync(string key)
        {
            if (key == null)
                return false;
            key = await FileSystemPathSanitizer.SanitizeAsync(key, _maxKeyLength);
            return await Task.Run(() => File.Exists(GetPath(key)));
        }

        public async ValueTask<byte[]> GetAsync(string key)
        {
            if (key == null) return null;
            if (!await ExistsAsync(key))
                return null; // We won't be able to open the stream for reading because the item doesn't exist
            using (var stream = await OpenAsync(key, new Options { FileAccess = FileAccess.Read, FileMode = FileMode.Open, FileShare = FileShare.Read }))
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public async Task SetAsync(string key, byte[] contents)
        {
            if (key == null || contents == null) return;
            using (var stream = await OpenAsync(key, new Options { FileAccess = FileAccess.Write, FileMode = FileMode.Create, FileShare = FileShare.None }))
                await stream.WriteAsync(contents, 0, contents.Length);
        }

        private string GetPath(string sanitizedKey) => sanitizedKey == null ? null : Path.Combine(_storageDirectory.FullName, Path.GetFileName(sanitizedKey));

        private async Task<Stream> OpenAsync(string key, Options options)
        {
            if (key == null)
                return null;
            key = await FileSystemPathSanitizer.SanitizeAsync(key, _maxKeyLength);
            return await Task.Run(() => File.Open(GetPath(key), options.FileMode, options.FileAccess, options.FileShare));
        }
    }
}