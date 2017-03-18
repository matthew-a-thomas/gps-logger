using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
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
    }
}
