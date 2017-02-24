using System.IO;

namespace GPS_Logger.LocalStorage
{
    public class PersistentStore : IPersistentStore
    {
        private readonly DirectoryInfo _storageDirectory;

        public PersistentStore(DirectoryInfo storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public bool Exists(string key) => File.Exists(GetPath(key));

        private string GetPath(string key) => key == null ? null : Path.Combine(_storageDirectory.FullName, Path.GetFileName(key));

        public Stream Open(string key, Options options) => key == null ? null : File.Open(GetPath(key), options.FileMode, options.FileAccess, options.FileShare);
    }
}