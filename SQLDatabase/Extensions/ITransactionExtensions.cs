using System;
using System.Linq;
using System.Threading.Tasks;

namespace SQLDatabase.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class ITransactionExtensions
    {
        public static async Task<T> GetScalarAsync<T>(this ITransaction transaction, Commands.Command command)
        {
            var results = transaction == null ? null : await transaction.GetResultsAsync(command);
            var first = results?[0];
            var result = first?.Values.First();
            try
            {
                return (T) Convert.ChangeType(result, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
