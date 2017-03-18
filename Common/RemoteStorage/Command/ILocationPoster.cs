using Common.RemoteStorage.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.RemoteStorage.Command
{
    public interface ILocationPoster
    {
        Task PostLocationAsync(byte[] identifier, Location location);
    }
}
