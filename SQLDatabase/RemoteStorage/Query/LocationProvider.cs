using Common.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Text;
using Common.RemoteStorage.Models;

namespace SQLDatabase.RemoteStorage.Query
{
    internal class LocationProvider : ILocationProvider
    {
        public IEnumerable<IdentifiedLocation> GetAllLocations(byte[] forIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
