using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
        public ConnectionProvider(IConfiguration config)
        {
            _connectionString = new SqlConnectionStringBuilder
            {
                UserID = config["user"],
                Password = config["password"],
                DataSource = config["server"],
                IntegratedSecurity = false,
                InitialCatalog = config["database"]
            }.ToString();
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
