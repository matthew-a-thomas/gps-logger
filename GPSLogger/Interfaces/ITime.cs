using System.Threading.Tasks;
using Common.Security.Signing;

namespace GPSLogger.Interfaces
{
    public interface ITime
    {
        Task<SignedMessage<long>> GetCurrentTimeAsync(TimeGetParameters request);
    }
}
