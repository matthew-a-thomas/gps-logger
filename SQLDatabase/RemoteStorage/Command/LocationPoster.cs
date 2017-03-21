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
        private readonly Transaction _transaction;

        public LocationPoster(
            IdentifierPoster identifierPoster,
            Transaction transaction
            )
        {
            _identifierPoster = identifierPoster;
            _transaction = transaction;
        }

        public async Task PostLocationAsync(byte[] identifier, Location location)
        {
            var id = await _identifierPoster.PostOrGetIdentifierAsync(identifier);
            await _transaction.ExecuteAsync(@"
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
        }
    }
}
