using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Errors;
using Common.Extensions;
using SQLDatabase.Commands;

namespace SQLDatabase.Errors
{
    /// <summary>
    /// Records exceptions into the database
    /// </summary>
    internal class ErrorHandler : IErrorHandler
    {
        // ReSharper disable once InconsistentNaming
        private const string ErrorReportingURL = "https://github.com/matthew-a-thomas/gps-logger/issues/new";

        private readonly Func<ITransaction> _transactionFactory;

        public ErrorHandler(Func<ITransaction> transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        private static (string hash, string message) ComputeHashCodeFor(Exception e)
        {
            if (e == null)
                return (hash: "", message: "");
            var message = e.ToString();
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(message)).ToHexString();
                return (hash: hash, message: message);
            }
        }

        private async Task<string> HandleInternalAsync(Exception e)
        {
            (var eHashCode, var eMessage) = ComputeHashCodeFor(e);
            try
            {
                var id = Guid.NewGuid().ToString();
                using (var transaction = _transactionFactory())
                {
                    await transaction.ExecuteAsync(Command.Create(
                        @"
insert into
    exceptions (
        id,
        message,
        hash
    )
values (
    @id,
    @message,
    @hash
)
",
                        new KeyValuePair<string, object>("@id", id),
                        new KeyValuePair<string, object>("@message", eMessage),
                        new KeyValuePair<string, object>("@hash", eHashCode)
                        ));
                    transaction.Commit();
                }
                return $"An error happened internally. Please report this as an issue here, making sure to include the ID \"{id}\" in your description: {ErrorReportingURL}";
            }
            catch (Exception e2)
            {
                (var e2HashCode, var _) = ComputeHashCodeFor(e2);
                return $"Ironically, there was an exception in some exception-handling code. The hash code of the exception is {e2HashCode}, and the hash code of the exception that was supposed to be handled is {eHashCode}. Please report this as an issue here, making sure to include these two hash codes in your description: {ErrorReportingURL}";
            }
        }

        public object Handle(Exception e) => HandleInternalAsync(e).WaitAndGet();
    }
}
