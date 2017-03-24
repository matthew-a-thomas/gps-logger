using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLDatabase.Commands;

namespace SQLDatabase
{
    /// <summary>
    /// A transaction that can be rolled back
    /// </summary>
    internal interface ITransaction : IDisposable
    {
        /// <summary>
        /// Commits this transaction
        /// </summary>
        void Commit();
        
        ValueTask<int> ExecuteAsync(Command command);
        ValueTask<IEnumerable<IReadOnlyDictionary<string, object>>> GetResultsAsync(Command command);
        ValueTask<object> GetScalarAsync(Command command);
    }
}
