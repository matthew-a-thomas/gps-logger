using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
            Sanitize(ref key);
            return File.Exists(GetPath(key));
        } 

        private string GetPath(string sanitizedKey) => sanitizedKey == null ? null : Path.Combine(_storageDirectory.FullName, Path.GetFileName(sanitizedKey));

        public Stream Open(string key, Options options)
        {
            if (key == null)
                return null;
            Sanitize(ref key);
            return File.Open(GetPath(key), options.FileMode, options.FileAccess, options.FileShare);
        }

        /// <summary>
        /// Turns the key into something outside the domain of folder/file/path names, in a case-insensitive way.
        /// It does this by making the key lowercase, turning it into a byte array, hashing it, then returning the hex string of the hash
        /// </summary>
        /// <param name="key"></param>
        private static void Sanitize(ref string key)
        {
            if (key == null)
                return;
            using (var hasher = MD5.Create())
                key = BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(key.ToLower())));
        }
    }
}