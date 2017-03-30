using System.Threading.Tasks;

namespace SQLDatabase.RemoteStorage.Command
{
    public interface IIdentifierPoster
    {
        ValueTask<int> PostOrGetIdentifierAsync(ITransaction transaction, byte[] identifier);
    }
}
