using System.Threading.Tasks;
using Common.Security;
using Common.Security.Signing;

namespace GPSLogger.Interfaces
{
    public interface ICredential
    {
        Task<SignedMessage<Credential<string>>> ProduceFromRequestAsync(CredentialGetParameters request);
    }
}
