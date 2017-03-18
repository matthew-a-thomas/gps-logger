using System.IO;

namespace SQLDatabase.Tests
{
    internal static class Helper
    {
        /// <summary>
        /// This method will throw exceptions until a "connection string" file is placed 
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            using (var reader = File.OpenText(@"C:\connection string.txt")) // TODO: Change this to a location that your build server is able to access, but that is kept secret from 
            {
                return reader.ReadToEnd();
            }
        }
    }
}
