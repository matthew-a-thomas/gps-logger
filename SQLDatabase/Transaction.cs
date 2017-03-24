using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using SQLDatabase.Commands;

namespace SQLDatabase
{
    /// <summary>
    /// Factory for commands that are within transactions
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Transaction : ITransaction
    {
        private readonly SqlTransaction _transaction;
        private readonly SqlConnection _connection;

        /// <summary>
        /// Creates a new factory for commands that are within transactions and executed against this SqlConnection
        /// </summary>
        public Transaction(ConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.CreateConnection();
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Commits this transaction, if commits are allowed
        /// </summary>
        public void Commit() => _transaction.Commit();

        public async ValueTask<int> ExecuteAsync(Command command) => await CreateSQLCommandFrom(command).ExecuteNonQueryAsync();

        public async ValueTask<IEnumerable<IReadOnlyDictionary<string, object>>> GetResultsAsync(Command command)
        {
            var results = new LinkedList<IReadOnlyDictionary<string, object>>();
            var sqlCommand = CreateSQLCommandFrom(command);
            await ProcessResultsAsync(sqlCommand, record => results.AddLast(record));
            return results;
        }

        public async ValueTask<object> GetScalarAsync(Command command) => await CreateSQLCommandFrom(command).ExecuteScalarAsync();

        /// <summary>
        /// Creates a new SqlCommand that is within this transaction
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        private SqlCommand CreateSQLCommandFrom(Command command)
        {
            var sqlCommand = _connection.CreateCommand();
            sqlCommand.Transaction = _transaction;
            sqlCommand.CommandText = command.CommandText;
            sqlCommand.Parameters.AddRange(command.Parameters.Select(kvp => new SqlParameter(kvp.Key, kvp.Value)).ToArray());
            return sqlCommand;
        }

        /// <summary>
        /// Rolls back this transaction
        /// </summary>
        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }

        /// <summary>
        /// Asynchronously processes results as they come back from a DataReader from the given command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="fieldsHandler"></param>
        /// <returns></returns>
        private static async Task ProcessResultsAsync(SqlCommand command, Action<IReadOnlyDictionary<string, object>> fieldsHandler)
        {
            // Get a SqlDataReader from the given command and parameters
            using (var reader = await command.ExecuteReaderAsync())
            {
                // Set up async functions from the reader
                var shouldLoopAsync = (Func<Task<bool>>)reader.ReadAsync;
                // ReSharper disable once InconsistentNaming
                var getAsync = new Func<SqlDataReader, Func<Task<IReadOnlyDictionary<string, object>>>>(_reader =>
                {
                    return () => Task.Run(() =>
                    {
                        var record = new Dictionary<string, object>();
                        for (var i = 0; i < _reader.FieldCount; ++i)
                        {
                            var name = _reader.GetName(i);
                            var value = _reader.GetValue(i);
                            record[name] = value;
                        }
                        return (IReadOnlyDictionary<string, object>)record;
                    });
                })(reader);

                // Turn the async functions into an IObservable
                var observable = getAsync.ToObservable(shouldLoopAsync);

                // Process the fields as they become available
                var finished = new ManualResetEventSlim(); // This will be our signal for when the observable completes
                using (observable.Subscribe(
                        onNext: fieldsHandler, // Invoke the handler for each set of fields
                        onCompleted: finished.Set // Set the gate when the observable completes
                    )) // Don't forget best practice of disposing IDisposables
                    // Asynchronously wait for the gate to be set
                    await Task.Run((Action)finished.Wait);
            }
        }
    }
}
