using System;
using Common.RemoteStorage.Command;
using Common.RemoteStorage.Models;
using System.Threading.Tasks;
using SQLDatabase.Extensions;
using System.Data.SqlClient;

namespace SQLDatabase.RemoteStorage.Command
{
    internal class LocationPoster : ILocationPoster
    {
        private readonly IdentifierPoster _identifierPoster;
        private readonly Func<Transaction> _transactionFactory;

        public LocationPoster(
            IdentifierPoster identifierPoster,
            Func<Transaction> transactionFactory
            )
        {
            _identifierPoster = identifierPoster;
            _transactionFactory = transactionFactory;
        }

        public async Task PostLocationAsync(byte[] identifier, Location location)
        {
            using (var transaction = _transactionFactory())
            {
                var id = await _identifierPoster.PostOrGetIdentifierAsync(transaction, identifier);
                await transaction.ExecuteAsync(@"
insert into
    locations (
        id,
        latitude,
        longitude
    )
values (
    @id,
    @latitude,
    @longitude
)
",
                    new SqlParameter("@id", id),
                    new SqlParameter("@latitude", location.Latitude),
                    new SqlParameter("@longitude", location.Longitude)
                );
                transaction.Commit();
            }
        }
    }
}
