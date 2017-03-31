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
        
        Task<int> ExecuteAsync(Command command);
        Task<IReadOnlyList<IReadOnlyDictionary<string, object>>> GetResultsAsync(Command command);
    }
}
