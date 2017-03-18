using Common.RemoteStorage.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.RemoteStorage.Query
{
    public interface ILocationProvider
    {
        IEnumerable<IdentifiedLocation> GetAllLocations(byte[] forIdentifier);
    }
}
