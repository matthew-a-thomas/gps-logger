using System.IO;

namespace GPS_Logger.LocalStorage
{
    public class Options
    {
        public FileMode FileMode { get; set; } = FileMode.Open;
        public FileAccess FileAccess { get; set; } = FileAccess.Read;
        public FileShare FileShare { get; set; } = FileShare.Read;
    }
}