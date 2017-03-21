using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLDatabase.Extensions
{
    public static class TransactionExtensions
    {
        /// <summary>
        /// Performs the given action against a command created with the other parameters, returning the result of your function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction"></param>
        /// <param name="function"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Do<T>(this Transaction transaction, Func<SqlCommand, T> function, string commandText = null, params SqlParameter[] parameters)
        {
            using (var command = transaction.CreateCommand())
            {
                if (commandText != null)
                {
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters);
                }
                return function(command);
            }
        }

        /// <summary>
        /// Asynchronously executes a command created with the other parameters, returning the number of things affected by the query
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync(this Transaction transaction, string commandText, params SqlParameter[] parameters)
        {
            return await Do(
                transaction,
                async command =>
                {
                    return await command.ExecuteNonQueryAsync();
                },
                commandText,
                parameters
            );
        }

        /// <summary>
        /// Asynchronously performs the given scalar query within this transaction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this Transaction transaction, string commandText, params SqlParameter[] parameters)
        {
            return await Do(
                transaction,
                async command =>
                {
                    var result = await command.ExecuteScalarAsync();
                    return (T)result;
                },
                commandText,
                parameters
            );
        }
        
        /// <summary>
        /// Asynchronously processes each record of the given reader using the given handler
        /// </summary>
        public static async Task ProcessResultsAsync(this Transaction transaction, string commandText, Action<IReadOnlyDictionary<string, object>> fieldsHandler, params SqlParameter[] parameters)
        {
            // Get a SqlDataReader from the given command and parameters
            using (var reader = await Do(
                transaction,
                async command => await command.ExecuteReaderAsync(),
                commandText,
                parameters
            ))
            {
                // Set up async functions from the reader
                var shouldLoopAsync = (Func<Task<bool>>)reader.ReadAsync;
                var getAsync = new Func<SqlDataReader, Func<Task<IReadOnlyDictionary<string, object>>>>(_reader =>
                {
                    return () => Task.Run(() =>
                    {
                        var record = new Dictionary<string, object>();
                        for (var i = 0; i < _reader.FieldCount; ++i)
                        {
                            var name = _reader.GetName(i);
                            var value = _reader.GetValue(i);
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
