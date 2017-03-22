using Common.Utilities;
using System.IO;
using System.Threading.Tasks;

namespace Common.LocalStorage
{
    public class PersistentStore : IPersistentStore
    {
        private readonly DirectoryInfo _storageDirectory;

        public PersistentStore(DirectoryInfo storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (key == null)
                return false;
            key = await FileSystemPathSanitizer.SanitizeAsync(key);
            return await Task.Run(() => File.Exists(GetPath(key)));
        } 

        private string GetPath(string sanitizedKey) => sanitizedKey == null ? null : Path.Combine(_storageDirectory.FullName, Path.GetFileName(sanitizedKey));

        public async Task<Stream> OpenAsync(string key, Options options)
        {
            if (key == null)
                return null;
            key = await FileSystemPathSanitizer.SanitizeAsync(key);
            return await Task.Run(() => File.Open(GetPath(key), options.FileMode, options.FileAccess, options.FileShare));
        }
    }
}