using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Command
{
    public interface IIdentifierPoster
    {
        Task<int> PostOrGetIdentifierAsync(ITransaction transaction, byte[] identifier);
    }
}
