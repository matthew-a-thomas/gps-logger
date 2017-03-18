using Common.LocalStorage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace SQLDatabase.Tests
{
    internal static class Helper
    {
        /// <summary>
        /// This method will throw exceptions until a "connection string" file is placed 
        /// </summary>
        /// <returns></returns>
        private static string GetConnectionString()
        {
            using (var reader = File.OpenText(@"\connection string.txt")) // TODO: Change this to a location that your build server is able to access, but that is kept secret from 
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Creates a new, open connection to the SQL server
        /// </summary>
        /// <returns></returns>
        public static SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(GetConnectionString());
            connection.Open();
            return connection;
        }
    }
}
