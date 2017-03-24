using System.Data.SqlClient;
using Common.Utilities;

namespace SQLDatabase
{
    /// <summary>
    /// Opens SQL connections
    /// </summary>
    internal class ConnectionProvider
    {
        private readonly IFactory<string, SqlConnection> _connectionFactory;
        private readonly string _connectionString;

        /// <summary>
        /// Creates a new provider which opens SQL connections using the given connection string
        /// </summary>
        public ConnectionProvider(ConnectionOptions connectionOptions, IFactory<ConnectionOptions, string> connectionStringFactory, IFactory<string, SqlConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _connectionString = connectionStringFactory.Create(connectionOptions);
        }

        /// <summary>
        /// Opens a new SQL connection
        /// </summary>
        /// <returns></returns>
        public SqlConnection CreateConnection() => _connectionFactory.Create(_connectionString);
    }
}
