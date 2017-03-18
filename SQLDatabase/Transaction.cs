using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SQLDatabase
{
    /// <summary>
    /// Factory for commands that are within transactions
    /// </summary>
    public class Transaction : IDisposable
    {
        private readonly SqlTransaction _transaction;
        private readonly SqlConnection _connection;

        /// <summary>
        /// Creates a new factory for commands that are within transactions and executed against this SqlConnection
        /// </summary>
        /// <param name="sqlConnection"></param>
        public Transaction(SqlConnection sqlConnection)
        {
            _connection = sqlConnection;
            _transaction = sqlConnection.BeginTransaction();
        }

        /// <summary>
        /// Creates a new SqlCommand that is within this transaction
        /// </summary>
        /// <returns></returns>
        public SqlCommand CreateCommand()
        {
            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            return command;
        }

        /// <summary>
        /// Disposes the transaction, rolling it back
        /// </summary>
        public void Dispose() => _transaction.Dispose();
    }
}
