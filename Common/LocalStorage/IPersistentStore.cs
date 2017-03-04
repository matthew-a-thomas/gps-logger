using System.IO;

namespace Common.LocalStorage
{
    public interface IPersistentStore
    {
        bool Exists(string key);

        Stream Open(string key, Options options);
    }
}