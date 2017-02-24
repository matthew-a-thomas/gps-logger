using System.IO;

namespace GPS_Logger.LocalStorage
{
    public interface IPersistentStore
    {
        bool Exists(string key);

        Stream Open(string key, Options options);
    }
}