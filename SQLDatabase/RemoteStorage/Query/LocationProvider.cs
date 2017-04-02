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

        public async Task<IEnumerable<IdentifiedLocation>> GetAllLocationsAsync(byte[] forIdentifier)
        {
            if (_transactionFactory == null)
                return null;
            using (var transaction = _transactionFactory())
            {
                if (transaction == null)
                    return null;
                var results = await transaction.GetResultsAsync(Commands.Command.Create(@"
select
    locations.latitude,
    locations.longitude,
    datediff(s, '1970-01-01 00:00:00', locations.timestamp) as unixTime
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
                    UnixTime = Convert.ToInt64(record["unixTime"])
                }).ToList();
                return locations;
            }
        }
    }
}
