using Common.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Text;
using Common.RemoteStorage.Models;
using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Query
{
    internal class LocationProvider : ILocationProvider
    {
        public async Task<IEnumerable<IdentifiedLocation>> GetAllLocationsAsync(byte[] forIdentifier)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }
    }
}
