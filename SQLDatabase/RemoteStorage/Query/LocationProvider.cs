using Common.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using Common.RemoteStorage.Models;
using System.Threading.Tasks;
using SQLDatabase.Extensions;
using System.Data.SqlClient;

namespace SQLDatabase.RemoteStorage.Query
{
    internal class LocationProvider : ILocationProvider
    {
        private readonly Transaction _transaction;

        public LocationProvider(Transaction transaction)
        {
            _transaction = transaction;
        }

        public async Task<IEnumerable<IdentifiedLocation>> GetAllLocationsAsync(byte[] forIdentifier)
        {
            var result = new LinkedList<IdentifiedLocation>();
            await _transaction.ProcessResultsAsync(@"
select
    locations.*
from
    identifiers
    join locations on
        identifiers.id = locations.id
where
    identifiers.hex = @hex
",
                record =>
                {
                    var location = new IdentifiedLocation
                    {
                        Identifier = forIdentifier,
                        Latitude = (double)record["latitude"],
                        Longitude = (double)record["longitude"],
                        Timestamp = (DateTime)record["timestamp"]
                    };
                    result.AddLast(location);
                },
                new SqlParameter("@hex", forIdentifier)
            );
            return result;
        }
    }
}
