using Common.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using Common.RemoteStorage.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SQLDatabase.RemoteStorage.Query
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LocationProvider : ILocationProvider
    {
        private readonly Func<ITransaction> _transactionFactory;

        public LocationProvider(Func<ITransaction> transactionFactory)
        {
            _transactionFactory = transactionFactory;
        }

        public async ValueTask<IEnumerable<IdentifiedLocation>> GetAllLocationsAsync(byte[] forIdentifier)
        {
            using (var transaction = _transactionFactory())
            {
                var results = await transaction.GetResultsAsync(Commands.Command.Create(@"
select
    locations.*
from
    identifiers
    join locations on
        identifiers.id = locations.id
where
    identifiers.hex = @hex
",
                    new KeyValuePair<string, object>("@hex", forIdentifier)
                ));
                var locations = results?.Select(record => new IdentifiedLocation
                {
                    Identifier = forIdentifier,
                    Latitude = Convert.ToDouble(record["latitude"]),
                    Longitude = Convert.ToDouble(record["longitude"]),
                    Timestamp = Convert.ToDateTime(record["timestamp"])
                }).ToList();
                return locations;
            }
        }
    }
}
