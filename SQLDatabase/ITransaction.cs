using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLDatabase.Commands;

namespace SQLDatabase
{
    /// <summary>
    /// A transaction that can be rolled back
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Commits this transaction
        /// </summary>
        void Commit();
        
        ValueTask<int> ExecuteAsync(Command command);
        ValueTask<IReadOnlyList<IReadOnlyDictionary<string, object>>> GetResultsAsync(Command command);
    }
}
