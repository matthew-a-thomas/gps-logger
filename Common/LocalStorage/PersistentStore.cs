using Common.Utilities;
using System.IO;

namespace Common.LocalStorage
{
    public class PersistentStore : IPersistentStore
    {
        private readonly DirectoryInfo _storageDirectory;

        public PersistentStore(DirectoryInfo storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public bool Exists(string key)
        {
            if (key == null)
                return false;
            FileSystemPathSanitizer.Sanitize(ref key);
            return File.Exists(GetPath(key));
        } 

        private string GetPath(string sanitizedKey) => sanitizedKey == null ? null : Path.Combine(_storageDirectory.FullName, Path.GetFileName(sanitizedKey));

        public Stream Open(string key, Options options)
        {
            if (key == null)
                return null;
            FileSystemPathSanitizer.Sanitize(ref key);
            return File.Open(GetPath(key), options.FileMode, options.FileAccess, options.FileShare);
        }
    }
}