using Common.RemoteStorage.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.RemoteStorage.Command
{
    public interface ILocationPoster
    {
        void PostLocation(byte[] identifier, Location location);
    }
}
