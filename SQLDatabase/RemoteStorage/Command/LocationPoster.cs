using System;
using System.Collections.Generic;
using Common.RemoteStorage.Command;
using Common.RemoteStorage.Models;
using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Command
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LocationPoster : ILocationPoster
    {
        private readonly IIdentifierPoster _identifierPoster;
        private readonly Func<ITransaction> _transactionFactory;

        public LocationPoster(
            IIdentifierPoster identifierPoster,
            Func<ITransaction> transactionFactory
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
                await transaction.ExecuteAsync(Commands.Command.Create(@"
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
                    new KeyValuePair<string, object>("@id", id),
                    new KeyValuePair<string, object>("@latitude", location.Latitude),
                    new KeyValuePair<string, object>("@longitude", location.Longitude)
                ));
                transaction.Commit();
            }
        }
    }
}
