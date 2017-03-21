using System;
using System.Data.SqlClient;

namespace SQLDatabase
{
    /// <summary>
    /// Factory for commands that are within transactions
    /// </summary>
    internal class Transaction : IDisposable
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
        /// Commits this transaction, if commits are allowed
        /// </summary>
        public void Commit() => _transaction.Commit();

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
        /// Rolls back this transaction
        /// </summary>
        public void Dispose() => _transaction.Dispose();
    }
}
