using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Security.Signing;
using GPSLogger.Models;

namespace GPSLogger.Interfaces
{
    public interface ILocation
    {
        Task<SignedMessage<bool>> AddLocationAsync(SignedMessage<Location> posted);

        Task<IEnumerable<Common.RemoteStorage.Models.Location>> GetLocationsForAsync(string id = "");
    }
}
