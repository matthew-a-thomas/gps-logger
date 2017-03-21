using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.RemoteStorage.Command
{
    public interface IIdentifierPoster
    {
        Task<int> PostOrGetIdentifierAsync(byte[] identifier);
    }
}
