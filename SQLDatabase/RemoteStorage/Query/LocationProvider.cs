using Common.RemoteStorage.Query;
using System;
using System.Collections.Generic;
using System.Text;
using Common.RemoteStorage.Models;

namespace SQLDatabase.RemoteStorage.Query
{
    public class LocationProvider : ILocationProvider
    {
        public IEnumerable<IdentifiedLocation> GetAllLocations(byte[] forIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
