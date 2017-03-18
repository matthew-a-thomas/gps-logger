using Common.RemoteStorage.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.RemoteStorage.Query
{
    public interface ILocationProvider
    {
        Task<IEnumerable<IdentifiedLocation>> GetAllLocationsAsync(byte[] forIdentifier);
    }
}
