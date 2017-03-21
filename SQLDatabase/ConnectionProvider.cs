﻿using System.Data.SqlClient;

namespace SQLDatabase
{
    /// <summary>
    /// Opens SQL connections
    /// </summary>
    internal class ConnectionProvider
    {
        private readonly string _connectionString;

        /// <summary>
        /// Creates a new provider which opens SQL connections using the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public ConnectionProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Opens a new SQL connection
        /// </summary>
        /// <returns></returns>
        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
